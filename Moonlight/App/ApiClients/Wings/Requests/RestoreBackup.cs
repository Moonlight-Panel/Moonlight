using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Wings.Requests;

public class RestoreBackup
{
    [JsonProperty("adapter")]
    public string Adapter { get; set; }

    [JsonProperty("truncate_directory")] public bool TruncateDirectory { get; set; } = false;
    [JsonProperty("download_url")] public string DownloadUrl { get; set; } = "";
}