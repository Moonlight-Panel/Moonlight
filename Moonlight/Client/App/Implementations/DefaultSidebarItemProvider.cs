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
                Icon = "bi bi-hdd-rack",
                Name = "Servers",
                Priority = 1,
                Target = "/servers"
            },
            new()
            {
                Icon = "bi bi-ethernet",
                Name = "Networks",
                Priority = 2,
                Target = "/networks"
            },
            
            
            
            new()
            {
                Icon = "bi bi-columns",
                Name = "Overview",
                Priority = 0,
                Group = "Admin",
                Target = "/admin",
                RequiresExactMatch = true
            },
            new()
            {
                Icon = "bi bi-hdd-rack",
                Name = "Servers",
                Priority = 1,
                Group = "Admin",
                Target = "/admin/servers"
            },
            new()
            {
                Icon = "bi bi-people",
                Name = "Users",
                Priority = 2,
                Group = "Admin",
                Target = "/admin/users"
            },
            new()
            {
                Icon = "bi bi-gear",
                Name = "System",
                Priority = 3,
                Group = "Admin",
                Target = "/admin/system"
            }
        ];
    }
}