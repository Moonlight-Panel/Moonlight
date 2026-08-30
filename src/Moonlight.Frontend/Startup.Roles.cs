using Moonlight.Frontend.Features.Roles;

namespace Moonlight.Frontend;

public static partial class Startup
{
    private static void AddRoles(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<RoleService>();
    }
}
