using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Requests;

public class CreateDomain
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("hosting_type")]
    public string HostingType { get; set; }

    [JsonProperty("hosting_settings")] public HostingSettings HostingSettings { get; set; } = new();

    [JsonProperty("owner_client")] public OwnerClient OwnerClient { get; set; } = new();

    [JsonProperty("plan")] public Plan Plan { get; set; } = new();
}

public partial class HostingSettings
{
    [JsonProperty("ftp_login")]
    public string FtpLogin { get; set; }

    [JsonProperty("ftp_password")]
    public string FtpPassword { get; set; }
}

public partial class OwnerClient
{
    [JsonProperty("id")]
    public long Id { get; set; }
}

public partial class Plan
{
    [JsonProperty("name")]
    public string Name { get; set; }
}