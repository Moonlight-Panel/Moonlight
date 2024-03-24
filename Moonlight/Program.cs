using System.Security.Cryptography.X509Certificates;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database;
using Moonlight.Core.Http.Middleware;
using Moonlight.Core.Services;

// Create needed storage directories
Directory.CreateDirectory(PathBuilder.Dir("storage"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "configs"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "assetOverrides"));

// Build config service
var configService = new ConfigService<CoreConfiguration>(
    PathBuilder.File("storage", "configs", "core.json")
);

var builder = WebApplication.CreateBuilder(args);

// Setup logging
Logger.Setup(
    logInConsole: true,
    logInFile: true,
    logPath: PathBuilder.File("storage", "logs", "moonlight.log"),
    isDebug: builder.Environment.IsDevelopment(),
    enableFileLogRotate: true,
    rotateLogNameTemplate: PathBuilder.File("storage", "logs", "moonlight.{0}.log")
);

builder.Logging.MigrateToMoonCore();
builder.Logging.AddConfiguration("{\"LogLevel\":{\"Default\":\"Information\",\"Microsoft.AspNetCore\":\"Warning\"}}");

// Configure http
if (builder.Environment.IsDevelopment())
    Logger.Info(
        "Disabling http pipeline configuration as the environment is set to development. All http endpoint config options in the core.json will be ignored");
else
{
    var httpConfig = configService.Get().Http;

    X509Certificate2? certificate = default;

    if (httpConfig.EnableSsl)
    {
        try
        {
            certificate = X509Certificate2.CreateFromPemFile(httpConfig.CertPath, httpConfig.KeyPath);
            
            Logger.Info($"Successfully loaded certificate '{certificate.FriendlyName}'");
        }
        catch (Exception e)
        {
            Logger.Fatal("An error occured while loading certificate");
            Logger.Fatal(e);
        }
    }
    
    builder.WebHost.ConfigureMoonCoreHttp(
        httpConfig.HttpPort,
        httpConfig.EnableSsl,
        httpConfig.HttpsPort
    );
}

// Build feature service and perform load
var featureService = new FeatureService(configService);
await featureService.Load();

// Build plugin service and perform load
var pluginService = new PluginService();
await pluginService.Load();

// Check database migrations
await DatabaseCheckHelper.Check(
    new DataContext(configService),
    false
);

// Add pre constructed services
builder.Services.AddSingleton(featureService);
builder.Services.AddSingleton(configService);
builder.Services.AddSingleton(pluginService);

// Feature hook
await featureService.PreInit(builder);

// Plugin hook
await pluginService.PreInitialize(builder);

var app = builder.Build();

// Feature hooks
await featureService.Init(app);
await featureService.UiInit();

// Plugin hooks
await pluginService.Initialized(app);

app.Services.StartBackgroundServices<Program>();

if (Environment.GetEnvironmentVariables().Contains("DEBUG_HTTP"))
{
    app.UseMiddleware<DebugLogMiddleware>();
}

app.Run();