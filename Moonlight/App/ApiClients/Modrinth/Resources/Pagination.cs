using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Modrinth.Resources;

public class Pagination
{
    [JsonProperty("hits")] public Project[] Hits { get; set; }

    [JsonProperty("offset")] public long Offset { get; set; }

    [JsonProperty("limit")] public long Limit { get; set; }

    [JsonProperty("total_hits")] public long TotalHits { get; set; }
}