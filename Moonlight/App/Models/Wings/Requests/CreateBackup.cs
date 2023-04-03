using Newtonsoft.Json;

namespace Moonlight.App.Models.Wings.Requests;

public class CreateBackup
{
    [JsonProperty("adapter")]
    public string Adapter { get; set; }
    
    [JsonProperty("uuid")]
    public Guid Uuid { get; set; }
    
    [JsonProperty("ignore")]
    public string Ignore { get; set; }
}