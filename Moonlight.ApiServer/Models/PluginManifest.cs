namespace Moonlight.ApiServer.Models;

public class PluginManifest
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string[] Dependencies { get; set; } = [];

    public string[] Scripts { get; set; } = [];
    public string[] Styles { get; set; } = [];

    public string[] BundledStyles { get; set; } = [];
    public Dictionary<string, string[]> Assemblies { get; set; } = new();
}