using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.CloudPanel.Requests;

public class InstallLetsEncrypt
{
    [JsonProperty("domainName")]
    public string DomainName { get; set; }
}