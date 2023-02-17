namespace Moonlight.App.Models.Node;

public class DockerStats
{
    public ContainerStats[] Containers { get; set; }
    public int NodeId { get; set; }
}

public class ContainerStats
{
    public Guid Name { get; set; }
    public long Memory { get; set; }
    public double Cpu { get; set; }
    public long NetworkIn { get; set; }
    public long NetworkOut { get; set; }
    public int NodeId { get; set; }
}