using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.CloudPanel.Requests;

public class AddPhpSite
{
    [JsonProperty("domainName")] public string DomainName { get; set; } = "";

    [JsonProperty("siteUser")] public string SiteUser { get; set; } = "";

    [JsonProperty("siteUserPassword")] public string SiteUserPassword { get; set; } = "";

    [JsonProperty("vHostTemplate")] public string VHostTemplate { get; set; } = "";

    [JsonProperty("phpVersion")] public string PhpVersion { get; set; } = "";
}