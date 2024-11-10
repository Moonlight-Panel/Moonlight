namespace Moonlight.ApiServer.Models;

public class ModuleModel
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string? DonateUrl { get; set; }
    public string? UpdateUrl { get; set; }

    public Dictionary<string, List<string>> Modules { get; set; } = new();
}