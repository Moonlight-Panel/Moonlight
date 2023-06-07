namespace Moonlight.App.ApiClients.Daemon.Resources;

public class DockerMetrics
{
    public Container[] Containers { get; set; } = Array.Empty<Container>();
}