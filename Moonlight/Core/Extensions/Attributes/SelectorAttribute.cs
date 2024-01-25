namespace Moonlight.Core.Extensions.Attributes;

public class SelectorAttribute : Attribute
{
    public string SelectorProp { get; set; } = "";
    public string DisplayProp { get; set; } = "";
    public bool UseDropdown { get; set; } = false;
}