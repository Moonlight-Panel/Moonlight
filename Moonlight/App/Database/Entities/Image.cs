namespace Moonlight.App.Database.Entities;

public class Image
{
    public int Id { get; set; }
    public Guid Uuid { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ConfigFiles { get; set; } = "{}";
    public string StopCommand { get; set; } = "";
    public string StartupDetection { get; set; } = "";
    public string InstallScript { get; set; } = "";
    public string InstallDockerImage { get; set; } = "";
    public string InstallEntrypoint { get; set; } = "";
    public string Startup { get; set; } = "";

    public List<DockerImage> DockerImages { get; set; } = new();
    public List<ImageVariable> Variables { get; set; } = new();
    public List<ImageTag> Tags { get; set; } = new();
}