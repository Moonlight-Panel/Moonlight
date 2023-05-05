using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Wings.Resources;

public class ListDirectory
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("created")]
    public DateTimeOffset Created { get; set; }

    [JsonProperty("modified")]
    public DateTimeOffset Modified { get; set; }
    
    [JsonProperty("size")]
    public long Size { get; set; }

    [JsonProperty("directory")]
    public bool Directory { get; set; }

    [JsonProperty("file")]
    public bool File { get; set; }

    [JsonProperty("symlink")]
    public bool Symlink { get; set; }

    [JsonProperty("mime")]
    public string Mime { get; set; }
}