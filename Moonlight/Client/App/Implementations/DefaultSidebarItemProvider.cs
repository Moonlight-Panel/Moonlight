using Moonlight.Client.App.Interfaces;
using Moonlight.Client.App.Models;

namespace Moonlight.Client.App.Implementations;

public class DefaultSidebarItemProvider : ISidebarItemProvider
{
    public SidebarItem[] GetItems()
    {
        return [
            new()
            {
                Icon = "bi bi-columns",
                Name = "Overview",
                Priority = 0,
                Target = "/",
                RequiresExactMatch = true
            },
            
            
            
            new()
            {
                Icon = "bi bi-columns",
                Name = "Overview",
                Priority = 0,
                Group = "Admin",
                Target = "/admin",
                RequiresExactMatch = true,
                Permission = "admin"
            },
            new()
            {
                Icon = "bi bi-people",
                Name = "Users",
                Priority = 2,
                Group = "Admin",
                Target = "/admin/users",
                Permission = "admin.users.get"
            },
            new()
            {
                Icon = "bi bi-key",
                Name = "API",
                Priority = 3,
                Group = "Admin",
                Target = "/admin/api",
                Permission = "admin.apikeys.get"
            },
            new()
            {
                Icon = "bi bi-gear",
                Name = "System",
                Priority = 3,
                Group = "Admin",
                Target = "/admin/system",
                Permission = "admin.system.info"
            }
        ];
    }
}