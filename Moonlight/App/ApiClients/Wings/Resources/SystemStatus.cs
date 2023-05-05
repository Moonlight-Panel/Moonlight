using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Wings.Resources;

public class SystemStatus
{
    [JsonProperty("architecture")]
    public string Architecture { get; set; }

    [JsonProperty("cpu_count")]
    public long CpuCount { get; set; }

    [JsonProperty("kernel_version")]
    public string KernelVersion { get; set; }

    [JsonProperty("os")]
    public string Os { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }
}