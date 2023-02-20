using Newtonsoft.Json;

namespace Moonlight.App.Models.Wings.Resources;

public class ServerDetailsResponse
{
    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("is_suspended")]
    public bool IsSuspended { get; set; }

    [JsonProperty("utilization")]
    public ServerDetailsResponseUtilization Utilization { get; set; }
    
    public class ServerDetailsResponseUtilization
    {
        [JsonProperty("memory_bytes")]
        public long MemoryBytes { get; set; }

        [JsonProperty("memory_limit_bytes")]
        public long MemoryLimitBytes { get; set; }

        [JsonProperty("cpu_absolute")]
        public double CpuAbsolute { get; set; }

        [JsonProperty("network")]
        public ServerDetailsResponseNetwork Network { get; set; }

        [JsonProperty("uptime")]
        public long Uptime { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("disk_bytes")]
        public long DiskBytes { get; set; }
    }
    
    public class ServerDetailsResponseNetwork
    {
        [JsonProperty("rx_bytes")]
        public long RxBytes { get; set; }

        [JsonProperty("tx_bytes")]
        public long TxBytes { get; set; }
    }
}