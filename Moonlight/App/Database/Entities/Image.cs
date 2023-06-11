using Newtonsoft.Json;

namespace Moonlight.App.Database.Entities;

public class Image
{
    [JsonIgnore]
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
    public int Allocations { get; set; } = 1; 
    public List<DockerImage> DockerImages { get; set; } = new();
    public List<ImageVariable> Variables { get; set; } = new();
    public string TagsJson { get; set; } = "";
    public string BackgroundImageUrl { get; set; } = "";
}