using System.ComponentModel;
using Newtonsoft.Json;

namespace Moonlight.Features.Servers.Configuration;

public class ServersConfiguration
{
    [JsonProperty("DisableServerKillWarning")]
    [Description("With this option you can globally disable the confirmation popup shown when killing a server")]
    public bool DisableServerKillWarning { get; set; } = false;
}