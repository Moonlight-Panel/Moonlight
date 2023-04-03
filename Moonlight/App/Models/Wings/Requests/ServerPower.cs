using Newtonsoft.Json;

namespace Moonlight.App.Models.Wings.Requests;

public class ServerPower
{
    [JsonProperty("action")]
    public string Action { get; set; }
}