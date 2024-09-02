using Microsoft.OpenApi.Models;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.App.Database;
using Moonlight.ApiServer.App.Helpers;
using Moonlight.ApiServer.App.Http.Middleware;
using Moonlight.ApiServer.App.Implementations;
using Moonlight.ApiServer.App.Implementations.Diagnose;
using Moonlight.ApiServer.App.Interfaces;
using Moonlight.ApiServer.App.PluginApi;
using Moonlight.ApiServer.App.Services;
using Moonlight.Shared.Models;

// Moonlight initialisation

if (args.Length > 0)
{
    Console.WriteLine("Starting with args: " + string.Join(" ", args));
    
    // Not a real "ef detection" but good enough for now
    // TODO: Detect startup args for ef

    ApplicationContext.IsRunningEfMigration = true;
}

// Prepare file system
Directory.CreateDirectory(PathBuilder.Dir("storage"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "plugins"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "clientPlugins"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));

// App configuration
var configService = new ConfigService<AppConfiguration>(
    PathBuilder.File("storage", "config.json")
);

ApplicationContext.ConfigService = configService;

var appConfiguration = configService.Get();

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

// Version print (TODO: that)
logger.LogInformation("Starting moonlight api server (Version: c05ea18@v2)");

var builder = WebApplication.CreateBuilder(args);

// Configure application logging
builder.Logging.ClearProviders();
builder.Logging.AddProviders(providers);

var logConfigPath = PathBuilder.File("storage", "logConfig.json");

// Ensure logging config, add a default one is missing
if (!File.Exists(logConfigPath))
{
    await File.WriteAllTextAsync(logConfigPath,
        "{\"LogLevel\":{\"Default\":\"Information\",\"Microsoft.AspNetCore\":\"Warning\"}}");
}

builder.Logging.AddConfiguration(await File.ReadAllTextAsync(logConfigPath));

// Mooncore di
builder.Services.AutoAddServices<Program>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton(configService);

// TODO: Make configurable, reconsider location
builder.Services.AddSingleton<IAuthenticationProvider, DefaultAuthenticationProvider>();

var pluginService = new PluginService(loggerFactory.CreateLogger<PluginService>(), loggerFactory);
builder.Services.AddSingleton(pluginService);
await pluginService.Load();

// Integrated implementations
pluginService.RegisterImplementation<IDiagnoseReporter, LogDiagnoseReporter>();
pluginService.RegisterImplementation<IDiagnoseReporter, HostDiagnoseReporter>();
pluginService.RegisterImplementation<IDiagnoseReporter, ConfigurationDiagnoseReporter>();
pluginService.RegisterImplementation<IDiagnoseReporter, PluginDiagnoseReporter>();

// Database
logger.LogInformation("Preparing database connection");
var databaseHelper = new DatabaseHelper(loggerFactory.CreateLogger<DatabaseHelper>());
builder.Services.AddSingleton(databaseHelper);

// Add db contexts here
builder.Services.AddDbContext<CoreDataContext>();
databaseHelper.AddDbContext<CoreDataContext>();

await pluginService.CallPlugins(x => x.OnAppBuilding(builder, databaseHelper));

// Continue with database
databaseHelper.GenerateMappings();
builder.Services.AddScoped(typeof(DatabaseRepository<>));

builder.Services.AddHttpContextAccessor();
var controllerBuilder = builder.Services.AddControllers();

foreach (var assembly in pluginService.PluginAssemblies)
    controllerBuilder.AddApplicationPart(assembly);

// API Docs
if (appConfiguration.Development.EnableApiDocs)
{
    // Configure swagger api specification generator and set the document title for the api docs to use
    builder.Services.AddSwaggerGen(options => options.SwaggerDoc("main", new OpenApiInfo()
    {
        Title = "Moonlight API"
    }));
}

var applicationService = new ApplicationService(loggerFactory.CreateLogger<ApplicationService>());
builder.Services.AddSingleton(applicationService);

var app = builder.Build();

applicationService.SetApplication(app);

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
app.UseMiddleware<PermissionLoadMiddleware>();
app.UseMiddleware<PermissionCheckMiddleware>();

app.MapControllers();
app.MapFallbackToFile("index.html");

// API Docs
if (appConfiguration.Development.EnableApiDocs)
    app.MapSwagger("/apidocs/swagger/{documentName}");

await pluginService.CallPlugins(x => x.OnAppConfiguring(app));

app.Run();