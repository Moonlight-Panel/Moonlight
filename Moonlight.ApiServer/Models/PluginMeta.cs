namespace Moonlight.ApiServer.Models;

public class PluginMeta
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string? DonationUrl { get; set; }
    public string? UpdateUrl { get; set; }

    public Dictionary<string, string[]> Binaries { get; set; } = new();
}