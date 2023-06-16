using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Modrinth.Resources;

public class VersionFile
{
    [JsonProperty("url")]
    public Uri Url { get; set; }

    [JsonProperty("filename")]
    public string Filename { get; set; }

    [JsonProperty("primary")]
    public bool Primary { get; set; }

    [JsonProperty("size")]
    public long Size { get; set; }

    [JsonProperty("file_type")]
    public string FileType { get; set; }
}