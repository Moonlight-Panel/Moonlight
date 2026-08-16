using Moonlight.Api.Features.Users;
using Moonlight.ApiSdk.Features.Users;

namespace Moonlight.Api;

public static partial class Startup
{
    private static void AddUsers(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserQueryService, UserQueryService>();
    }
}