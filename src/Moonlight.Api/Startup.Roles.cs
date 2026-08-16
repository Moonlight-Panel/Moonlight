using Moonlight.Api.Features.Roles;
using Moonlight.ApiSdk.Features.Roles;

namespace Moonlight.Api;

public static partial class Startup
{
    private static void AddRoles(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRoleQueryService, RoleQueryService>();
    }
}