using Microsoft.OpenApi.Models;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Extensions;
using MoonCore.Extended.Helpers;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database;
using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Http.Middleware;

// Prepare file system
Directory.CreateDirectory(PathBuilder.Dir("storage"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "plugins"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "clientPlugins"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));

// Configuration
var configService = new ConfigService<AppConfiguration>(
    PathBuilder.File("storage", "config.json")
);

var config = configService.Get();

ApplicationStateHelper.SetConfiguration(configService);

// Build pre run logger
var providers = LoggerBuildHelper.BuildFromConfiguration(configuration =>
{
    configuration.Console.Enable = true;
    configuration.Console.EnableAnsiMode = true;

    configuration.FileLogging.Enable = true;
    configuration.FileLogging.Path = PathBuilder.File("storage", "logs", "moonlight.log");
    configuration.FileLogging.EnableLogRotation = true;
    configuration.FileLogging.RotateLogNameTemplate = PathBuilder.File("storage", "logs", "moonlight.log.{0}");
});

using var loggerFactory = new LoggerFactory(providers);
var logger = loggerFactory.CreateLogger("Startup");

// Fancy start console output... yes very fancy :>
var rainbow = new Crayon.Rainbow(0.5);
foreach (var c in "Moonlight")
{
    Console.Write(
        rainbow
            .Next()
            .Bold()
            .Text(c.ToString())
    );
}

Console.WriteLine();

var builder = WebApplication.CreateBuilder(args);

// Configure application logging
builder.Logging.ClearProviders();
builder.Logging.AddProviders(providers);

// Logging levels
var logConfigPath = PathBuilder.File("storage", "logConfig.json");

// Ensure logging config, add a default one is missing
if (!File.Exists(logConfigPath))
{
    await File.WriteAllTextAsync(logConfigPath,
        "{\"LogLevel\":{\"Default\":\"Information\",\"Microsoft.AspNetCore\":\"Warning\"}}");
}

builder.Logging.AddConfiguration(await File.ReadAllTextAsync(logConfigPath));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(configService);

builder.Services.AddSingleton<JwtHelper>();
builder.Services.AutoAddServices<Program>();

// OAuth2
builder.Services.AddSingleton<TokenHelper>();

builder.Services.AddHttpClient();
builder.Services.AddOAuth2Consumer(configuration =>
{
    configuration.ClientId = config.Authentication.ClientId;
    configuration.ClientSecret = config.Authentication.ClientSecret;
    configuration.AuthorizationRedirect =
        config.Authentication.AuthorizationRedirect ?? $"{config.PublicUrl}/api/auth/handle";

    configuration.AccessEndpoint = config.Authentication.AccessEndpoint ?? $"{config.PublicUrl}/oauth2/access";
    configuration.RefreshEndpoint = config.Authentication.RefreshEndpoint ?? $"{config.PublicUrl}/oauth2/refresh";
    
    if (config.Authentication.UseLocalOAuth2Service)
    {
        configuration.AuthorizationEndpoint = config.Authentication.AuthorizationRedirect ?? $"{config.PublicUrl}/oauth2/authorize";
    }
    else
    {
        if(config.Authentication.AuthorizationUri == null)
            logger.LogWarning("The 'AuthorizationUri' for the oauth2 client is not set. If you want to use an external oauth2 provider, you need to specify this url. If you want to use the local oauth2 service, set 'UseLocalOAuth2Service' to true");

        configuration.AuthorizationEndpoint = config.Authentication.AuthorizationUri!;
    }
});

if (config.Authentication.UseLocalOAuth2Service)
{
    logger.LogInformation("Using local oauth2 provider");
    
    builder.Services.AddOAuth2Provider(configuration =>
    {
        configuration.AccessSecret = config.Authentication.AccessSecret;
        configuration.RefreshSecret = config.Authentication.RefreshSecret;

        configuration.ClientId = config.Authentication.ClientId;
        configuration.ClientId = config.Authentication.ClientSecret;
        configuration.CodeSecret = config.Authentication.CodeSecret;
        configuration.AuthorizationRedirect =
            config.Authentication.AuthorizationRedirect ?? $"{config.PublicUrl}/api/auth/handle";
        configuration.AccessTokenDuration = 60;
        configuration.RefreshTokenDuration = 3600;
    });
}

// Database
var databaseHelper = new DatabaseHelper(
    loggerFactory.CreateLogger<DatabaseHelper>()
);

builder.Services.AddSingleton(databaseHelper);
builder.Services.AddScoped(typeof(DatabaseRepository<>));
builder.Services.AddScoped(typeof(CrudHelper<,>));

builder.Services.AddDbContext<CoreDataContext>();
databaseHelper.AddDbContext<CoreDataContext>();

databaseHelper.GenerateMappings();

// API Docs
if (configService.Get().Development.EnableApiDocs)
{
    // Configure swagger api specification generator and set the document title for the api docs to use
    builder.Services.AddSwaggerGen(options => options.SwaggerDoc("main", new OpenApiInfo()
    {
        Title = "Moonlight API"
    }));
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await databaseHelper.EnsureMigrated(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<ApiErrorMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<AuthorizationMiddleware>();

app.MapControllers();

app.MapFallbackToFile("index.html");

// API Docs
if (configService.Get().Development.EnableApiDocs)
    app.MapSwagger("/api/swagger/{documentName}");

app.Run();