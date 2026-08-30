using LucideBlazor;
using Moonlight.FrontendSdk.Features.Auth;

namespace Moonlight.Frontend.Features.Users;

public class UsersPermissionProvider : IPermissionProvider
{
    public Task ApplyChangesAsync(List<Permission> items)
    {
        items.Add(new Permission()
        {
            DisplayName = "Read Users",
            Identifier = "users.read",
            Group = "Users",
            Description = "View user details",
            Icon = typeof(UsersIcon)
        });

        items.Add(new Permission()
        {
            DisplayName = "Write Users",
            Identifier = "users.write",
            Group = "Users",
            Description = "Create, update and delete users",
            Icon = typeof(UsersIcon)
        });
        
        return Task.CompletedTask;
    }
}