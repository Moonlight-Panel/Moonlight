namespace Moonlight.App.Database.Entities;

public class Server
{
    public int Id { get; set; }
    public Guid Uuid { get; set; }
    public string Name { get; set; } = "";
    public int Cpu { get; set; }
    public long Memory { get; set; }
    public long Disk { get; set; }
    public Image Image { get; set; } = null!;
    public int DockerImageIndex { get; set; } = 0;
    public string OverrideStartup { get; set; } = "";
    public bool Installing { get; set; } = false;
    public bool Suspended { get; set; } = false;

    public List<ServerVariable> Variables { get; set; } = new();
    public List<ServerBackup> Backups { get; set; } = new();
    public List<NodeAllocation> Allocations { get; set; } = new();
    public NodeAllocation? MainAllocation { get; set; } = null;
    public Node Node { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public bool IsCleanupException { get; set; } = false;
}