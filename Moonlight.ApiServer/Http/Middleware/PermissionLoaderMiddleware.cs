using System.Text.Json;
using MoonCore.Authentication;
using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Http.Middleware;

public class PermissionLoaderMiddleware
{
    private readonly RequestDelegate Next;

    public PermissionLoaderMiddleware(RequestDelegate next)
    {
        Next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await Load(context);
        await Next(context);
    }

    private Task Load(HttpContext context)
    {
        if(context.User is not PermClaimsPrinciple permClaimsPrinciple)
            return Task.CompletedTask;
        
        if(permClaimsPrinciple.IdentityModel is not User user)
            return Task.CompletedTask;

        permClaimsPrinciple.Permissions = JsonSerializer.Deserialize<string[]>(user.PermissionsJson) ?? [];
        return Task.CompletedTask;
    }
}