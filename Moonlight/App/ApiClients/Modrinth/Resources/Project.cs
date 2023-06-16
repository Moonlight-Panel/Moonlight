using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Modrinth.Resources;

public class Project
{
    [JsonProperty("project_id")] public string ProjectId { get; set; }

    [JsonProperty("project_type")] public string ProjectType { get; set; }

    [JsonProperty("slug")] public string Slug { get; set; }

    [JsonProperty("author")] public string Author { get; set; }

    [JsonProperty("title")] public string Title { get; set; }

    [JsonProperty("description")] public string Description { get; set; }

    [JsonProperty("categories")] public string[] Categories { get; set; }

    [JsonProperty("display_categories")] public string[] DisplayCategories { get; set; }

    [JsonProperty("versions")] public string[] Versions { get; set; }

    [JsonProperty("downloads")] public long Downloads { get; set; }

    [JsonProperty("follows")] public long Follows { get; set; }

    [JsonProperty("icon_url")] public string IconUrl { get; set; }

    [JsonProperty("date_created")] public DateTimeOffset DateCreated { get; set; }

    [JsonProperty("date_modified")] public DateTimeOffset DateModified { get; set; }

    [JsonProperty("latest_version")] public string LatestVersion { get; set; }

    [JsonProperty("license")] public string License { get; set; }

    [JsonProperty("client_side")] public string ClientSide { get; set; }

    [JsonProperty("server_side")] public string ServerSide { get; set; }

    [JsonProperty("gallery")] public Uri[] Gallery { get; set; }

    [JsonProperty("featured_gallery")] public Uri FeaturedGallery { get; set; }

    [JsonProperty("color")] public long? Color { get; set; }
}