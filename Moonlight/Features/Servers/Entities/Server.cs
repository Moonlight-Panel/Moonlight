using Moonlight.Core.Database.Entities;

namespace Moonlight.Features.Servers.Entities;

public class Server
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public User Owner { get; set; }

    public ServerImage Image { get; set; }
    public int DockerImageIndex { get; set; } = 0;

    public string? OverrideStartupCommand { get; set; }

    public int Cpu { get; set; } = 100;
    public int Memory { get; set; }
    public int Disk { get; set; }
    public bool UseVirtualDisk { get; set; } = false;

    public ServerNode Node { get; set; }
    public ServerNetwork? Network { get; set; }
    public bool DisablePublicNetwork { get; set; } = false;

    public ServerAllocation MainAllocation { get; set; }
    public List<ServerAllocation> Allocations { get; set; } = new();

    public List<ServerVariable> Variables { get; set; } = new();
    public List<ServerBackup> Backups { get; set; } = new();

    public List<ServerSchedule> Schedules { get; set; } = new();
}