using Moonlight.App.Models.Misc;
using Moonlight.App.Plugin.UI.Servers;
using Moonlight.App.Plugin.UI.Webspaces;

namespace Moonlight.App.Plugin;

public abstract class MoonlightPlugin
{
    public string Name { get; set; } = "N/A";
    public string Author { get; set; } = "N/A";
    public string Version { get; set; } = "N/A";
    
    public Func<ServerPageContext, Task>? OnBuildServerPage { get; set; }
    public Func<WebspacePageContext, Task>? OnBuildWebspacePage { get; set; }
    public Func<IServiceCollection, Task>? OnBuildServices { get; set; }
    public Func<List<MalwareScan>, Task<List<MalwareScan>>>? OnBuildMalwareScans { get; set; }
}