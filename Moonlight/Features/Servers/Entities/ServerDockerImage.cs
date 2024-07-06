namespace Moonlight.Features.Servers.Entities;

public class ServerDockerImage
{
    public int Id { get; set; }
    
    public string DisplayName { get; set; } = "";
    public string Name { get; set; } = "";

    public bool AutoPull { get; set; } = true;
}