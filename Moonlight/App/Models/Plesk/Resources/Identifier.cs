using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Resources;

public class Identifier
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("guid")]
    public Guid Guid { get; set; }
}