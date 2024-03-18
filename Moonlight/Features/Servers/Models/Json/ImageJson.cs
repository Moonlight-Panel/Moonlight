namespace Moonlight.Features.Servers.Models.Json;

public class ImageJson
{
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

    public List<ImageVariable> Variables { get; set; } = new();

    public int DefaultDockerImage { get; set; } = 0;
    public bool AllowDockerImageChange { get; set; } = false;
    public List<ImageDockerImage> DockerImages { get; set; } = new();
    
    public class ImageVariable
    {
        public string Key { get; set; } = "";
        public string DefaultValue { get; set; } = "";

        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";

        public bool AllowView { get; set; } = false;
        public bool AllowEdit { get; set; } = false;

        public string? Filter { get; set; }
    }
    public class ImageDockerImage // Weird name xd
    {
        public string DisplayName { get; set; } = "";
        public string Name { get; set; } = "";
        public bool AutoPull { get; set; }
    }
}