using Moonlight.App.Configuration;
using Moonlight.App.Helpers;
using Newtonsoft.Json;

namespace Moonlight.App.Services;

public class ConfigService
{
    private readonly string Path = PathBuilder.File("storage", "config.json");
    private ConfigV1 Data;

    public ConfigService()
    {
        Reload();
    }

    public void Reload()
    {
        if(!File.Exists(Path))
            File.WriteAllText(Path, "{}");

        var text = File.ReadAllText(Path);
        Data = JsonConvert.DeserializeObject<ConfigV1>(text) ?? new();
        text = JsonConvert.SerializeObject(Data, Formatting.Indented);
        File.WriteAllText(Path, text);
    }

    public ConfigV1 Get()
    {
        return Data;
    }
}