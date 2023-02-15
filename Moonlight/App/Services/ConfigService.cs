using System.Text;
using Logging.Net;
using Microsoft.Extensions.Primitives;

namespace Moonlight.App.Services;

public class ConfigService : IConfiguration
{
    private IConfiguration Configuration;

    public bool DebugMode { get; private set; } = false;
    
    public ConfigService()
    {
        Configuration = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(File.ReadAllText("..\\..\\appsettings.json")))
        ).Build();

        // Env vars
        var debugVar = Environment.GetEnvironmentVariable("ML_DEBUG");

        if (debugVar != null)
            DebugMode = bool.Parse(debugVar);
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