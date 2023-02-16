using Newtonsoft.Json;

namespace Moonlight.App.Helpers;

public class ConfigUtils
{
    public static void SaveConfigurationAsJson(IConfiguration configuration, string filePath)
    {
        // Serialize the configuration to a JSON object
        var jsonObject = new Dictionary<string, object>();
        foreach (var section in configuration.GetChildren())
        {
            SerializeSection(section, jsonObject);
        }

        // Convert the JSON object to a JSON string
        var jsonString = JsonConvert.SerializeObject(jsonObject);

        // Write the JSON string to a file
        File.WriteAllText(filePath, jsonString);
    }

    private static void SerializeSection(IConfigurationSection section, IDictionary<string, object> jsonObject)
    {
        var children = section.GetChildren();

        if (!children.Any())
        {
            // Leaf node
            jsonObject[section.Key] = section.Value;
        }
        else
        {
            // Non-leaf node
            var childObject = new Dictionary<string, object>();
            foreach (var childSection in children)
            {
                SerializeSection(childSection, childObject);
            }
            jsonObject[section.Key] = childObject;
        }
    }
}