namespace Moonlight.Features.Servers.Entities;

public class ServerNode
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string Token { get; set; }
    public int HttpPort { get; set; }
    public int FtpPort { get; set; }

    public List<ServerAllocation> Allocations { get; set; } = new();
}