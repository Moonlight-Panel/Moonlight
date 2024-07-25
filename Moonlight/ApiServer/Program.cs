using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.App.Configuration;
using Moonlight.ApiServer.App.Database;
using Moonlight.ApiServer.App.Http.Middleware;
using Moonlight.ApiServer.App.Implementations;
using Moonlight.ApiServer.App.Interfaces;

// Moonlight initialisation

Console.WriteLine(string.Join(" ", args));

// Prepare file system
Directory.CreateDirectory(PathBuilder.Dir("storage"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "plugins"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));

// App configuration
var configService = new ConfigService<AppConfiguration>(
    PathBuilder.File("storage", "config.json")
);

var appConfiguration = configService.Get();

// Build pre run logger
var providers = LoggerBuildHelper.BuildFromConfiguration(new()
{
    Console = new()
    {
        Enable = true,
        EnableAnsiMode = true
    },
    FileLogging = new()
    {
        Enable = true,
        Path = PathBuilder.File("storage", "logs", "moonlight.log"),
        EnableLogRotation = true,
        RotateLogNameTemplate = PathBuilder.File("storage", "logs", "moonlight.log.{0}")
    }
});

var preLoggerFactory = new LoggerFactory(providers);
var logger = preLoggerFactory.CreateLogger("Startup");

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

// Version print (TODO: that)
logger.LogInformation("Starting moonlight api server (Version: c05ea18@v2)");

var builder = WebApplication.CreateBuilder(args);

// Configure application logging
builder.Logging.ClearProviders();
builder.Logging.AddProviders(providers);

var logConfigPath = PathBuilder.File("storage", "logConfig.json");

// Ensure logging config, add a default one is missing
if (!File.Exists(logConfigPath))
    await File.WriteAllTextAsync(logConfigPath, "{\"LogLevel\":{\"Default\":\"Information\",\"Microsoft.AspNetCore\":\"Warning\"}}");

builder.Logging.AddConfiguration(await File.ReadAllTextAsync(logConfigPath));

// Mooncore di
builder.Services.AutoAddServices<Program>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton(configService);

// TODO: Make configurable, reconsider location
builder.Services.AddSingleton<IAuthenticationProvider, DefaultAuthenticationProvider>();

// Database
logger.LogInformation("Preparing database connection");
var databaseHelper = new DatabaseHelper(preLoggerFactory.CreateLogger<DatabaseHelper>());
builder.Services.AddSingleton(databaseHelper);

// Add db contexts here
builder.Services.AddDbContext<CoreDataContext>();
databaseHelper.AddDbContext<CoreDataContext>();

// Continue with database
databaseHelper.GenerateMappings();
builder.Services.AddScoped(typeof(DatabaseRepository<>));

builder.Services.AddControllers();
builder.Services.AddRazorPages();

var app = builder.Build();

// Continue with database
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

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();