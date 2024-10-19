using System.Text.Json;
using Microsoft.OpenApi.Models;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Extensions;
using MoonCore.Extended.Helpers;
using MoonCore.Extended.OAuth2.ApiServer;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Models;
using MoonCore.PluginFramework.Services;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Helpers.Authentication;
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
    configuration.ClientId = config.Authentication.OAuth2.ClientId;
    configuration.ClientSecret = config.Authentication.OAuth2.ClientSecret;
    configuration.AuthorizationRedirect =
        config.Authentication.OAuth2.AuthorizationRedirect ?? $"{config.PublicUrl}/api/auth/handle";

    configuration.AccessEndpoint = config.Authentication.OAuth2.AccessEndpoint ?? $"{config.PublicUrl}/oauth2/access";
    configuration.RefreshEndpoint = config.Authentication.OAuth2.RefreshEndpoint ?? $"{config.PublicUrl}/oauth2/refresh";
    
    if (config.Authentication.UseLocalOAuth2)
    {
        configuration.AuthorizationEndpoint = config.Authentication.OAuth2.AuthorizationRedirect ?? $"{config.PublicUrl}/oauth2/authorize";
    }
    else
    {
        if(config.Authentication.OAuth2.AuthorizationUri == null)
            logger.LogWarning("The 'AuthorizationUri' for the oauth2 client is not set. If you want to use an external oauth2 provider, you need to specify this url. If you want to use the local oauth2 service, set 'UseLocalOAuth2Service' to true");

        configuration.AuthorizationEndpoint = config.Authentication.OAuth2.AuthorizationUri!;
    }
});

if (config.Authentication.UseLocalOAuth2)
{
    logger.LogInformation("Using local oauth2 provider");
    
    builder.Services.AddOAuth2Provider(configuration =>
    {
        configuration.AccessSecret = config.Authentication.LocalOAuth2.AccessSecret;
        configuration.RefreshSecret = config.Authentication.LocalOAuth2.RefreshSecret;

        configuration.ClientId = config.Authentication.OAuth2.ClientId;
        configuration.ClientSecret = config.Authentication.OAuth2.ClientSecret;
        configuration.CodeSecret = config.Authentication.LocalOAuth2.CodeSecret;
        configuration.AuthorizationRedirect =
            config.Authentication.OAuth2.AuthorizationRedirect ?? $"{config.PublicUrl}/api/auth/handle";
        configuration.AccessTokenDuration = 60;
        configuration.RefreshTokenDuration = 3600;
    });
}

builder.Services.AddTokenAuthentication(configuration =>
{
    configuration.AccessSecret = config.Authentication.AccessSecret;
    configuration.DataLoader = async (data, provider, context) =>
    {
        if (!data.TryGetValue("userId", out var userIdStr) || !int.TryParse(userIdStr, out var userId))
            return false;

        var userRepo = provider.GetRequiredService<DatabaseRepository<User>>();
        var user = userRepo.Get().FirstOrDefault(x => x.Id == userId);

        if (user == null)
            return false;
        
        // OAuth2 - Check external
        if (false && DateTime.UtcNow > user.RefreshTimestamp)
        {
            var tokenConsumer = new TokenConsumer(user.AccessToken, user.RefreshToken, user.RefreshTimestamp,
                async refreshToken =>
                {
                    var oauth2Service = context.RequestServices.GetRequiredService<OAuth2Service>();

                    var accessData = await oauth2Service.RefreshAccess(refreshToken);

                    user.AccessToken = accessData.AccessToken;
                    user.RefreshToken = accessData.RefreshToken;
                    user.RefreshTimestamp = DateTime.UtcNow.AddSeconds(accessData.ExpiresIn);
                
                    userRepo.Update(user);

                    return new TokenPair()
                    {
                        AccessToken = user.AccessToken,
                        RefreshToken = user.RefreshToken
                    };
                });

            //await tokenConsumer.GetAccessToken();
            //TODO: API CALL (modular)
        }
        
        // Load permissions, handle empty values
        var permissions = JsonSerializer.Deserialize<string[]>(
            string.IsNullOrEmpty(user.PermissionsJson) ? "[]" : user.PermissionsJson
        ) ?? [];

        // Save permission state
        context.User = new PermClaimsPrinciple(permissions, user);
        
        return true;
    };
});

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

// Implementation service
var implementationService = new ImplementationService();



builder.Services.AddSingleton(implementationService);

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

app.UseTokenAuthentication(_ => {});

app.UseMiddleware<AuthorizationMiddleware>();

app.MapControllers();

app.MapFallbackToFile("index.html");

// API Docs
if (configService.Get().Development.EnableApiDocs)
    app.MapSwagger("/api/swagger/{documentName}");

app.Run();