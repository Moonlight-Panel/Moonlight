namespace Moonlight.Features.Servers.Entities;

public class ServerAllocation
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = "";
    public int Port { get; set; }
    public string Note { get; set; } = "";
}