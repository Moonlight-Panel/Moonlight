using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Modrinth.Resources;

public class Version
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("version_number")]
    public string VersionNumber { get; set; }

    [JsonProperty("changelog")]
    public string Changelog { get; set; }

    [JsonProperty("dependencies")]
    public object[] Dependencies { get; set; }

    [JsonProperty("game_versions")]
    public object[] GameVersions { get; set; }

    [JsonProperty("version_type")]
    public string VersionType { get; set; }

    [JsonProperty("loaders")]
    public object[] Loaders { get; set; }

    [JsonProperty("featured")]
    public bool Featured { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("requested_status")]
    public string RequestedStatus { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("project_id")]
    public string ProjectId { get; set; }

    [JsonProperty("author_id")]
    public string AuthorId { get; set; }

    [JsonProperty("date_published")]
    public DateTime DatePublished { get; set; }

    [JsonProperty("downloads")]
    public long Downloads { get; set; }

    [JsonProperty("changelog_url")]
    public object ChangelogUrl { get; set; }

    [JsonProperty("files")]
    public VersionFile[] Files { get; set; }
}