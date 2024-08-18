namespace Moonlight.Shared.Http.Resources.Admin.Sys;

public class SystemInfoResponse
{
    public string OsName { get; set; }
    public long MemoryUsage { get; set; }
    public int CpuUsage { get; set; }
    public TimeSpan Uptime { get; set; }
}