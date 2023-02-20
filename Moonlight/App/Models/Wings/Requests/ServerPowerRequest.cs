using Newtonsoft.Json;

namespace Moonlight.App.Models.Wings.Requests;

public class ServerPowerRequest
{
    [JsonProperty("action")]
    public string Action { get; set; }
}