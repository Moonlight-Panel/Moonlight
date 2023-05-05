using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Paper.Resources;

public class PaperVersions
{
    [JsonProperty("project_id")]
    public string ProjectId { get; set; }

    [JsonProperty("project_name")]
    public string ProjectName { get; set; }

    [JsonProperty("version_groups")]
    public List<string> VersionGroups { get; set; }

    [JsonProperty("versions")]
    public List<string> Versions { get; set; }
}