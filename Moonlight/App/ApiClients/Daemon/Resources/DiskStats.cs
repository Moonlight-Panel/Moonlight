namespace Moonlight.App.ApiClients.Daemon.Resources;

public class DiskStats
{
    public long FreeBytes { get; set; }
    public string DriveFormat { get; set; }
    public string Name { get; set; }
    public long TotalSize { get; set; }
}