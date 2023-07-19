using Moonlight.App.Configuration;
using Moonlight.App.Helpers;
using Moonlight.App.Services.Files;
using Newtonsoft.Json;

namespace Moonlight.App.Services;

public class ConfigService
{
    private readonly StorageService StorageService;
    private readonly string Path;
    private ConfigV1 Configuration;

    public bool DebugMode { get; private set; } = false;
    public bool SqlDebugMode { get; private set; } = false;

    public ConfigService(StorageService storageService)
    {
        StorageService = storageService;
        StorageService.EnsureCreated();

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ML_CONFIG_PATH")))
            Path = Environment.GetEnvironmentVariable("ML_CONFIG_PATH")!;
        else
            Path = PathBuilder.File("storage", "configs", "config.json");

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
        if (!File.Exists(Path))
        {
            File.WriteAllText(Path, "{}");
        }

        Configuration = JsonConvert.DeserializeObject<ConfigV1>(
            File.ReadAllText(Path)
        ) ?? new ConfigV1();
        
        File.WriteAllText(Path, JsonConvert.SerializeObject(Configuration, Formatting.Indented));
    }

    public void Save(ConfigV1 configV1)
    {
        Configuration = configV1;
        Save();
    }

    public void Save()
    {
        if (!File.Exists(Path))
        {
            File.WriteAllText(Path, "{}");
        }
        
        File.WriteAllText(Path, JsonConvert.SerializeObject(Configuration, Formatting.Indented));
        
        Reload();
    }

    public ConfigV1 Get()
    {
        return Configuration;
    }
}