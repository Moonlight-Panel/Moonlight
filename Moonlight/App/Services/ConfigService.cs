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
        Save();
    }

    public void Save()
    {
        var text = JsonConvert.SerializeObject(Data, Formatting.Indented);
        File.WriteAllText(Path, text);
    }

    public ConfigV1 Get()
    {
        return Data;
    }

    public string GetDiagnoseJson()
    {
        var text = File.ReadAllText(Path);
        var data = JsonConvert.DeserializeObject<ConfigV1>(text) ?? new();

        // Security token
        data.Security.Token = "";

        // Database
        if (string.IsNullOrEmpty(data.Database.Password))
            data.Database.Password = "WAS EMPTY";
        else
            data.Database.Password = "WAS NOT EMPTY";
        
        // Mailserver
        if (string.IsNullOrEmpty(data.MailServer.Password))
            data.MailServer.Password = "WAS EMPTY";
        else
            data.MailServer.Password = "WAS NOT EMPTY";

        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }
}