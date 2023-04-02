using Newtonsoft.Json;

namespace Moonlight.App.Models.Wings.Requests;

public class DeleteFiles
{
    [JsonProperty("root")]
    public string Root { get; set; }

    [JsonProperty("files")] public List<string> Files { get; set; } = new();
}