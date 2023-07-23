using Microsoft.AspNetCore.Components;

namespace Moonlight.App.Plugin.UI.Servers;

public class ServerTab
{
    public string Name { get; set; }
    public string Route { get; set; }
    public string Icon { get; set; }
    public RenderFragment Component { get; set; }
}