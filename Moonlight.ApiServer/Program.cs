using Microsoft.OpenApi.Models;
using MoonCore.Extended.Abstractions;
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

if(app.Environment.IsDevelopment())
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