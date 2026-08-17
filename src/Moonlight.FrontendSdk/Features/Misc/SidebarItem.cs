namespace Moonlight.FrontendSdk.Features.Misc;

public class SidebarItem
{
    public string Name { get; init; }
    public string Path { get; init; }
    public bool IsExactPath { get; init; }
    public string? Group { get; init; }
    public int Order { get; init; }
    public Type IconType { get; init; }
}