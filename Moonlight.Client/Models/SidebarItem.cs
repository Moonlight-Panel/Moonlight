namespace Moonlight.Client.Models;

public class SidebarItem
{
    public string Icon { get; set; }
    public string Name { get; set; }
    public string? Group { get; set; }
    public string Path { get; set; }
    public int Priority { get; set; }
    public bool RequiresExactMatch { get; set; } = false;
}