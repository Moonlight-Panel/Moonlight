using Moonlight.App.Configuration;
using Moonlight.App.Helpers;
using Moonlight.App.Services.Files;
using Newtonsoft.Json;

namespace Moonlight.App.Services;

public class ConfigService
{
    private readonly StorageService StorageService;
    private ConfigV1 Configuration;

    public bool DebugMode { get; private set; } = false;
    public bool SqlDebugMode { get; private set; } = false;

    public ConfigService(StorageService storageService)
    {
        StorageService = storageService;
        StorageService.EnsureCreated();

        Reload();

        // Env vars
        var debugVar = Environment.GetEnvironmentVariable("ML_DEBUG");

        if (debugVar != null)
            DebugMode = bool.Parse(debugVar);

        if (DebugMode)
            Logger.Debug("Debug mode enabled");
        
        var sqlDebugVar = Environment.GetEnvironmentVariable("ML_SQL_DEBUG");

        if (sqlDebugVar != null)
            SqlDebugMode = bool.Parse(sqlDebugVar);

        if (SqlDebugMode)
            Logger.Debug("Sql debug mode enabled");
    }

    public void Reload()
    {
        var path = PathBuilder.File("storage", "configs", "config.json");
        
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "{}");
        }

        Configuration = JsonConvert.DeserializeObject<ConfigV1>(
            File.ReadAllText(path)
        ) ?? new ConfigV1();
        
        File.WriteAllText(path, JsonConvert.SerializeObject(Configuration, Formatting.Indented));
    }

    public void Save(ConfigV1 configV1)
    {
        Configuration = configV1;
        Save();
    }

    public void Save()
    {
        var path = PathBuilder.File("storage", "configs", "config.json");
        
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "{}");
        }
        
        File.WriteAllText(path, JsonConvert.SerializeObject(Configuration, Formatting.Indented));
        
        Reload();
    }

    public ConfigV1 Get()
    {
        return Configuration;
    }
}