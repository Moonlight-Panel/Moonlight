using Moonlight.Features.ServiceManagement.Entities;

namespace Moonlight.Features.Servers.Entities;

public class Server
{
    public int Id { get; set; }
    public Service Service { get; set; }

    public int Cpu { get; set; }
    public int Memory { get; set; }
    public int Disk { get; set; }
    
    public ServerImage Image { get; set; }
    public int DockerImageIndex { get; set; }
    public string? OverrideStartupCommand { get; set; }
    public List<ServerVariable> Variables { get; set; }

    public ServerNode Node { get; set; }
    public ServerAllocation MainAllocation { get; set; }
    public List<ServerAllocation> Allocations { get; set; } = new();
}