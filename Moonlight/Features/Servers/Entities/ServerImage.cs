namespace Moonlight.Features.Servers.Entities;

public class ServerImage
{
    public int Id { get; set; }
    
    public string Name { get; set; } = "";

    public string Author { get; set; } = "";
    public string? UpdateUrl { get; set; }
    public string? DonateUrl { get; set; }

    public string StartupCommand { get; set; } = "echo Startup command here";
    public string OnlineDetection { get; set; } = "Running";
    public string StopCommand { get; set; } = "^C";

    public string InstallShell { get; set; } = "/bin/bash";
    public string InstallDockerImage { get; set; } = "debian:latest";
    public string InstallScript { get; set; } = "#! /bin/bash\necho Done";

    public string ParseConfiguration { get; set; } = "[]";
    public int AllocationsNeeded { get; set; } = 1;

    public List<ServerImageVariable> Variables { get; set; } = new();

    public int DefaultDockerImage { get; set; } = 0;
    public bool AllowDockerImageChange { get; set; } = false;
    public List<ServerDockerImage> DockerImages { get; set; } = new();
}