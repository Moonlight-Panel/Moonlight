using Moonlight.Client.Interfaces;
using Moonlight.Client.Models;

namespace Moonlight.Client.Implementations;

public class DefaultSidebarItemProvider : ISidebarItemProvider
{
    public SidebarItem[] Get()
    {
        return
        [
            // User
            new SidebarItem()
            {
                Icon = "bi bi-columns",
                Name = "Overview",
                Path = "/",
                Priority = 0,
                RequiresExactMatch = true
            },
            
            // Admin
            new SidebarItem()
            {
                Icon = "bi bi-columns",
                Name = "Overview",
                Group = "Admin",
                Path = "/admin",
                Priority = 0,
                RequiresExactMatch = true,
                Permission = "admin.overview"
            },
            new SidebarItem()
            {
                Icon = "bi bi-people",
                Name = "Users",
                Group = "Admin",
                Path = "/admin/users",
                Priority = 1,
                RequiresExactMatch = false,
                Permission = "admin.users.read"
            },
            new SidebarItem()
            {
                Icon = "bi bi-key",
                Name = "API",
                Group = "Admin",
                Path = "/admin/api",
                Priority = 2,
                RequiresExactMatch = false,
                Permission = "admin.api.read"
            },
            new SidebarItem()
            {
                Icon = "bi bi-gear",
                Name = "System",
                Group = "Admin",
                Path = "/admin/system",
                Priority = 3,
                RequiresExactMatch = false,
                Permission = "admin.system.info"
            },
        ];
    }
}