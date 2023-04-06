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

    [JsonProperty("hosting_settings")]
    public HostingSettingsModel HostingSettings { get; set; }

    [JsonProperty("owner_client")]
    public OwnerClientModel OwnerClient { get; set; }

    [JsonProperty("plan")]
    public PlanModel Plan { get; set; }
    
    public partial class HostingSettingsModel
    {
        [JsonProperty("ftp_login")]
        public string FtpLogin { get; set; }

        [JsonProperty("ftp_password")]
        public string FtpPassword { get; set; }
    }

    public partial class OwnerClientModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }
    }

    public partial class PlanModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}