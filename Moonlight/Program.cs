using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database;
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

// Log rotate
var latestLogPath = PathBuilder.File("storage", "logs", "latest.log");

int GetCurrentRotateId()
{
    var counter = 0;

    foreach (var file in Directory.GetFiles(PathBuilder.Dir("storage", "logs")))
    {
        var fileName = Path.GetFileName(file);
        var fileNameParts = fileName.Split(".");
        
        if(fileNameParts.Length < 3)
            continue;

        var numberPart = fileNameParts[1];
        
        if(!int.TryParse(numberPart, out var number))
            continue;

        if (number > counter)
            counter = number;
    }

    return counter + 1;
}

if(File.Exists(latestLogPath))
    File.Move(latestLogPath, PathBuilder.File("storage", "logs", $"moonlight.{GetCurrentRotateId()}.log"));

// Setup logging
Logger.Setup(
    logInConsole: true,
    logInFile: true,
    logPath: latestLogPath,
    isDebug: builder.Environment.IsDevelopment()
);

builder.Logging.MigrateToMoonCore();

var config =
    new ConfigurationBuilder().AddJsonString(
        "{\"LogLevel\":{\"Default\":\"Information\",\"Microsoft.AspNetCore\":\"Warning\"}}");
builder.Logging.AddConfiguration(config.Build());

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

app.Run();