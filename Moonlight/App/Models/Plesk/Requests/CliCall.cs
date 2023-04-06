using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Requests;

public class CliCall
{
    [JsonProperty("params")] public List<string> Params { get; set; } = new();

    [JsonProperty("env")] public Dictionary<string, string> Env { get; set; } = new();
}