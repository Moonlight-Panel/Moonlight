using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Wings.Requests;

public class CreateServer
{
    [JsonProperty("uuid")]
    public Guid Uuid { get; set; }
    
    [JsonProperty("start_on_completion")]
    public bool StartOnCompletion { get; set; }
}