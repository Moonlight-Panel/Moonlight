namespace Moonlight.Client.App.Models;

public class SidebarItem
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Target { get; set; }
    public int Priority { get; set; } = 0;
    public bool RequiresExactMatch { get; set; } = false;
    public string Group { get; set; } = string.Empty;
}