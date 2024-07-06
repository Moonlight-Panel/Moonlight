using System.Security.Cryptography.X509Certificates;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Logging;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database;
using Moonlight.Core.Http.Middleware;
using Moonlight.Core.Services;
using MySqlConnector;

// Create needed storage directories
Directory.CreateDirectory(PathBuilder.Dir("storage"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "configs"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "assetOverrides"));

// Build config service
var configService = new ConfigService<CoreConfiguration>(
    PathBuilder.File("storage", "configs", "core.json")
);

// Build pre run logger
var loggerProviders = LoggerBuildHelper.BuildFromConfiguration(new()
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
        RotateLogNameTemplate = PathBuilder.File("storage", "logs", "moonlight.{0}.log")
    }
});

var preRunLoggerFactory = new LoggerFactory();
preRunLoggerFactory.AddProviders(loggerProviders);
var preRunLogger = preRunLoggerFactory.CreateLogger<Program>();

preRunLogger.LogInformation("Initializing moonlight");

// Initialisation
var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddProviders(loggerProviders);
builder.Logging.AddConfiguration("{\"LogLevel\":{\"Default\":\"Information\",\"Microsoft.AspNetCore\":\"Warning\"}}");

// Configure http
if (builder.Environment.IsDevelopment())
{
    preRunLogger.LogInformation("Disabling http pipeline configuration as the environment is set to development. All http endpoint config options in the core.json will be ignored");
}
else
{
    var httpConfig = configService.Get().Http;

    X509Certificate2? certificate = default;

    if (httpConfig.EnableSsl)
    {
        try
        {
            certificate = X509Certificate2.CreateFromPemFile(httpConfig.CertPath, httpConfig.KeyPath);

            preRunLogger.LogInformation("Successfully loaded certificate {name}", certificate.Subject);
        }
        catch (Exception e)
        {
            preRunLogger.LogCritical("An error occured while loading certificate: {e}", e);
        }
    }

    builder.WebHost.ConfigureMoonCoreHttp(
        httpConfig.HttpPort,
        httpConfig.EnableSsl,
        httpConfig.HttpsPort,
        certificate
    );
}

// Build feature service and perform load
var featureService = new FeatureService(
    configService,
    preRunLoggerFactory.CreateLogger<FeatureService>()
);

await featureService.Load();

// Build plugin service and perform load
var pluginService = new PluginService(
    preRunLoggerFactory.CreateLogger<PluginService>()
);

await pluginService.Load();

try
{
    // Check database migrations
    await DatabaseCheckHelper.Check(
        preRunLoggerFactory.CreateLogger<DataContext>(),
        new DataContext(configService)
    );
}
catch (MySqlException e)
{
    bool IsBootException(MySqlException e)
    {
        if (e.InnerException is EndOfStreamException eosException)
        {
            if (!eosException.Message.Contains("read 4 header bytes"))
                return false;
        }
        else if (e.InnerException is MySqlEndOfStreamException endOfStreamException)
        {
            if (!endOfStreamException.Message.Contains("An incomplete response was received from the server"))
                return false;
        }
        else
            throw e;

        return true;
    }

    if (IsBootException(e))
    {
        preRunLogger.LogWarning("The mysql server appears to be still booting up. Exiting...");

        Environment.Exit(1);
        return;
    }
}

// Add pre constructed services
builder.Services.AddSingleton(featureService);
builder.Services.AddSingleton(configService);
builder.Services.AddSingleton(pluginService);

// Feature hook
await featureService.PreInit(builder, pluginService, preRunLoggerFactory);

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