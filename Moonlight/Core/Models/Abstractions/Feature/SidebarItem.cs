namespace Moonlight.Core.Models.Abstractions.Feature;

public class SidebarItem
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Target { get; set; }
    public bool IsAdmin { get; set; } = false;
    public bool NeedsExactMath { get; set; } = false;
    public int Index { get; set; } = 0;
}