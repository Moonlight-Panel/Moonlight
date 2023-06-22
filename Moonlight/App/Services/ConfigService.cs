using System.Text;
using Microsoft.Extensions.Primitives;
using Moonlight.App.Helpers;
using Moonlight.App.Services.Files;

namespace Moonlight.App.Services;

public class ConfigService : IConfiguration
{
    private readonly StorageService StorageService;

    private IConfiguration Configuration;

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
        Configuration = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(
                    File.ReadAllText(
                        PathBuilder.File("storage", "configs", "config.json")
                    )
                )
            )).Build();
    }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        return Configuration.GetChildren();
    }

    public IChangeToken GetReloadToken()
    {
        return Configuration.GetReloadToken();
    }

    public IConfigurationSection GetSection(string key)
    {
        return Configuration.GetSection(key);
    }

    public string this[string key]
    {
        get => Configuration[key];
        set => Configuration[key] = value;
    }
}