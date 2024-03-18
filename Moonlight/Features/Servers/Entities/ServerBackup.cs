namespace Moonlight.Features.Servers.Entities;

public class ServerBackup
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long Size { get; set; }
    public bool Successful { get; set; }
    public bool Completed { get; set; }
}