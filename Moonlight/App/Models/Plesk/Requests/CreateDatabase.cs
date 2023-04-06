using Newtonsoft.Json;

namespace Moonlight.App.Models.Plesk.Requests;

public class CreateDatabase
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("parent_domain")] public ParentDomainModel ParentDomain { get; set; } = new();

    [JsonProperty("server_id")]
    public int ServerId { get; set; }
    
    public class ParentDomainModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}