using Newtonsoft.Json;

namespace Moonlight.App.ApiClients.Wings.Resources;

public class ServerDetails
{
    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("is_suspended")]
    public bool IsSuspended { get; set; }

    [JsonProperty("utilization")]
    public ServerDetailsUtilization Utilization { get; set; }
    
    public class ServerDetailsUtilization
    {
        [JsonProperty("memory_bytes")]
        public long MemoryBytes { get; set; }

        [JsonProperty("memory_limit_bytes")]
        public long MemoryLimitBytes { get; set; }

        [JsonProperty("cpu_absolute")]
        public double CpuAbsolute { get; set; }

        [JsonProperty("network")]
        public ServerDetailsNetwork Network { get; set; }

        [JsonProperty("uptime")]
        public long Uptime { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("disk_bytes")]
        public long DiskBytes { get; set; }
    }
    
    public class ServerDetailsNetwork
    {
        [JsonProperty("rx_bytes")]
        public long RxBytes { get; set; }

        [JsonProperty("tx_bytes")]
        public long TxBytes { get; set; }
    }
}