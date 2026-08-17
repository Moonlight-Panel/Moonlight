using LucideBlazor;
using Moonlight.FrontendSdk.Features.Misc;

namespace Moonlight.Frontend.Features.Users;

public class UsersSidebarProvider : ISidebarItemProvider
{
    public Task ApplyChangesAsync(List<SidebarItem> items)
    {
        items.Add(new SidebarItem()
        {
            Group = "Admin",
            IconType = typeof(UsersIcon),
            Name = "Users",
            Path = "/admin/users"
        });
        
        return Task.CompletedTask;
    }
}