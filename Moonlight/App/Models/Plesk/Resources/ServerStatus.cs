using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Resources;

public class ServerStatus
{
    [JsonProperty("platform")]
    public string Platform { get; set; }

    [JsonProperty("hostname")]
    public string Hostname { get; set; }

    [JsonProperty("guid")]
    public Guid Guid { get; set; }

    [JsonProperty("panel_version")]
    public string PanelVersion { get; set; }
}