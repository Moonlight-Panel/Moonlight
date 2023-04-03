using Newtonsoft.Json;

namespace Moonlight.App.Models.Wings.Requests;

public class CompressFiles
{
    [JsonProperty("root")]
    public string Root { get; set; }

    [JsonProperty("files")]
    public string[] Files { get; set; }
}