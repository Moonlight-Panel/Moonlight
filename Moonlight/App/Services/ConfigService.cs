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
        
        ApplyEnvironmentVariables("Moonlight", Data);
        
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

    private void ApplyEnvironmentVariables(string prefix, object objectToLookAt)
    {
        foreach (var property in objectToLookAt.GetType().GetProperties())
        {
            var envName = $"{prefix}_{property.Name}";
            
            if (property.PropertyType.Assembly == GetType().Assembly)
            {
                ApplyEnvironmentVariables(envName, property.GetValue(objectToLookAt)!);
            }
            else
            {
                if(!Environment.GetEnvironmentVariables().Contains(envName))
                    continue;

                var envValue = Environment.GetEnvironmentVariable(envName)!;

                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(objectToLookAt, envValue);
                }
                else if (property.PropertyType == typeof(int))
                {
                    if(!int.TryParse(envValue, out int envIntValue))
                        continue;
                    
                    property.SetValue(objectToLookAt, envIntValue);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    if(!bool.TryParse(envValue, out bool envBoolValue))
                        continue;
                    
                    property.SetValue(objectToLookAt, envBoolValue);
                }
            }
        }
    }
}