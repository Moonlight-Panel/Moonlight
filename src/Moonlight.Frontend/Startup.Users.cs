using Moonlight.Frontend.Features.Users;
using Moonlight.FrontendSdk.Features.Auth;
using Moonlight.FrontendSdk.Features.Misc;

namespace Moonlight.Frontend;

public static partial class Startup
{
    private static void AddUsers(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPermissionProvider, UsersPermissionProvider>();
        builder.Services.AddSingleton<ISidebarItemProvider, UsersSidebarProvider>();
        builder.Services.AddScoped<UserService>();
    }
}