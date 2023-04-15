using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Resources;

public class DatabaseServer
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("host")]
    public string Host { get; set; }

    [JsonProperty("port")]
    public int Port { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("db_count")]
    public int DbCount { get; set; }

    [JsonProperty("is_default")]
    public bool IsDefault { get; set; }

    [JsonProperty("is_local")]
    public bool IsLocal { get; set; }
}