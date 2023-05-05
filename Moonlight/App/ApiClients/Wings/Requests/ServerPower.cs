using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Wings.Requests;

public class ServerPower
{
    [JsonProperty("action")]
    public string Action { get; set; }
}