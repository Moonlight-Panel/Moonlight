namespace Moonlight.Features.Servers.Entities;

public class ServerNode
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public string Fqdn { get; set; } = "";
    public int HttpPort { get; set; } = 8080;
    public int FtpPort { get; set; } = 2021;
    public bool Ssl { get; set; } = false;
    
    public string Token { get; set; } = "";

    public List<ServerAllocation> Allocations { get; set; } = new();
}