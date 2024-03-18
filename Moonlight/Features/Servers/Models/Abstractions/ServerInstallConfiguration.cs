namespace Moonlight.Features.Servers.Models.Abstractions;

public class ServerInstallConfiguration
{
    public string DockerImage { get; set; }
    public string Shell { get; set; }
    public string Script { get; set; }
}