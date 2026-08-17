namespace Moonlight.FrontendSdk.Features.Misc;

public interface ISidebarItemProvider
{
    public Task ApplyChangesAsync(List<SidebarItem> items);
}