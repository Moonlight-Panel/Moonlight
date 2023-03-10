using Newtonsoft.Json;

namespace Moonlight.App.Models.Notifications;

public class BasicWSModel
{
    [JsonProperty("action")]
    public string Action { get; set; }
}