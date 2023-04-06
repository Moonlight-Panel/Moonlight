using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Resources;

public class Database
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}