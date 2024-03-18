namespace Moonlight.Features.Servers.Entities;

public class ServerImage
{
    public int Id { get; set; }
    
    public string Name { get; set; } = "";

    public string Author { get; set; } = "";
    public string? UpdateUrl { get; set; }
    public string? DonateUrl { get; set; }

    public string StartupCommand { get; set; } = "";
    public string OnlineDetection { get; set; } = "";
    public string StopCommand { get; set; } = "";

    public string InstallShell { get; set; } = "";
    public string InstallDockerImage { get; set; } = "";
    public string InstallScript { get; set; } = "";

    public string ParseConfiguration { get; set; } = "[]";
    public int AllocationsNeeded { get; set; } = 1;

    public List<ServerImageVariable> Variables { get; set; } = new();

    public int DefaultDockerImage { get; set; } = 0;
    public bool AllowDockerImageChange { get; set; } = false;
    public List<ServerDockerImage> DockerImages { get; set; } = new();
}