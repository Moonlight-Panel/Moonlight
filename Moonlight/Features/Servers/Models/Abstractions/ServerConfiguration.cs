namespace Moonlight.Features.Servers.Models.Abstractions;

public class ServerConfiguration
{
    public int Id { get; set; }

    public LimitsData Limits { get; set; }
    public ImageData Image { get; set; }
    public NetworkData Network { get; set; }
    public AllocationData MainAllocation { get; set; }
    public List<AllocationData> Allocations { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new();
    
    public class LimitsData
    {
        public int Cpu { get; set; }
        public int Memory { get; set; }
        public int Disk { get; set; }
        public bool UseVirtualDisk { get; set; }
    }
    
    public class ImageData
    {
        public string DockerImage { get; set; }
        public bool PullDockerImage { get; set; }
        public string StartupCommand { get; set; }
        public string StopCommand { get; set; }
        public string OnlineDetection { get; set; }
        public string ParseConfigurations { get; set; }
    }

    public class AllocationData
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
    }
    
    public class NetworkData
    {
        public bool Enable { get; set; }
        public int Id { get; set; }
        public bool DisablePublic { get; set; }
    }
}