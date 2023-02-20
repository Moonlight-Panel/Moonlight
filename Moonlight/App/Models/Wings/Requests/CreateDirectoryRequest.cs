using Newtonsoft.Json;

namespace Moonlight.App.Models.Wings.Requests;

public class CreateDirectoryRequest
{
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("path")]
    public string Path { get; set; }
}