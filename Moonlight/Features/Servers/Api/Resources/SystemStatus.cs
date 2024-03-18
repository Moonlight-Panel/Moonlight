namespace Moonlight.Features.Servers.Api.Resources;

public class SystemStatus
{
    public int Containers { get; set; }
    public string OperatingSystem { get; set; }
    public string Version { get; set; }
    public HardwareInformationData Hardware { get; set; }
    
    public class HardwareInformationData
    {
        public CpuCoreData[] Cores { get; set; }
        public MemoryData Memory { get; set; }
        public DiskData Disk { get; set; }
        public TimeSpan Uptime { get; set; }
    
        public class CpuCoreData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Usage { get; set; }
        }
    
        public class DiskData
        {
            public long Free { get; set; }
            public long Total { get; set; }
        }
    
        public class MemoryData
        {
            public long Total { get; set; }
            public long Available { get; set; }
            public long Free { get; set; }
            public long Cached { get; set; }
            public long Swap { get; set; }
            public long SwapFree { get; set; }
        }
    }
}