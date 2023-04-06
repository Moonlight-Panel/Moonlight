using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Resources;

public class CreateResult
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("guid")]
    public Guid Guid { get; set; }
}