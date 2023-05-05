using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Wings.Requests;

public class DeleteFiles
{
    [JsonProperty("root")]
    public string Root { get; set; }

    [JsonProperty("files")] public List<string> Files { get; set; } = new();
}