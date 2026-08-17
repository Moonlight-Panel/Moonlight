using Moonlight.Frontend.Features.Users;
using Moonlight.FrontendSdk.Features.Misc;

namespace Moonlight.Frontend;

public static partial class Startup
{
    private static void AddUsers(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISidebarItemProvider, UsersSidebarProvider>();
        builder.Services.AddScoped<UserService>();
    }
}