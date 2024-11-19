namespace Moonlight.ApiServer.Models;

public class PluginManifest
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string[] Dependencies { get; set; } = [];

    public Dictionary<string, string[]> Entrypoints { get; set; } = new();
}