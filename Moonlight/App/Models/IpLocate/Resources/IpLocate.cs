using Newtonsoft.Json;

namespace Moonlight.App.Models.IpLocate.Resources;

public class IpLocate
{
    [JsonProperty("query")]
    public string Query { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("countryCode")]
    public string CountryCode { get; set; }

    [JsonProperty("region")]
    public string Region { get; set; }

    [JsonProperty("regionName")]
    public string RegionName { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("zip")]
    public string Zip { get; set; }

    [JsonProperty("lat")]
    public double Lat { get; set; }

    [JsonProperty("lon")]
    public double Lon { get; set; }

    [JsonProperty("timezone")]
    public string Timezone { get; set; }

    [JsonProperty("isp")]
    public string Isp { get; set; }

    [JsonProperty("org")]
    public string Org { get; set; }

    [JsonProperty("as")]
    public string As { get; set; }
}