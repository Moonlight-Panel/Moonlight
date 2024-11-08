namespace Moonlight.Shared.Http.Responses.Admin.Sys;

public class SystemOverviewResponse
{
    public int CpuUsage { get; set; }
    public long MemoryUsage { get; set; }
    public string OperatingSystem { get; set; }
    public TimeSpan Uptime { get; set; }
}