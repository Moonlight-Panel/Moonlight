using System.Text.Json;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.App.Configuration;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Extensions;

namespace Moonlight.ApiServer.App.Http.Middleware;

public class PermissionLoadMiddleware
{
    private readonly RequestDelegate Next;

    public PermissionLoadMiddleware(RequestDelegate next)
    {
        Next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await Handle(context);
        await Next(context);
    }

    private async Task Handle(HttpContext context)
    {
        if(!context.Request.Headers.ContainsKey("Authorization"))
            return;

        var headerValue = context.Request.Headers["Authorization"].ToString();
        
        if(string.IsNullOrEmpty(headerValue))
            return;

        var jwtHelper = context.RequestServices.GetRequiredService<JwtHelper>();
        var configService = context.RequestServices.GetRequiredService<ConfigService<AppConfiguration>>();
        var secret = configService.Get().Security.Token;
        
        if(!await jwtHelper.Validate(secret, headerValue, "userLogin"))
            return;

        var jwtData = await jwtHelper.Decode(secret, headerValue);
        
        if(!jwtData.TryGetValue("UserId", out var userIdText))
            return;
        
        if(!int.TryParse(userIdText, out var userId))
            return;

        var userRepo = context.RequestServices.GetRequiredService<DatabaseRepository<User>>();
        var user = userRepo.Get().FirstOrDefault(x => x.Id == userId);
        
        if(user == null)
            return;
        
        context.SetCurrentUser(user);
        
        // Load permissions
        var permissionsJson = string.IsNullOrEmpty(user.PermissionsJson) ? "[]" : user.PermissionsJson;
        var usersPermissions = JsonSerializer.Deserialize<string[]>(permissionsJson) ?? [];

        // Add meta permissions
        var finalPermissions = usersPermissions.Concat(
            ["meta.authenticated"]
        ).ToArray();
        
        context.SetPermissions(finalPermissions);
    }
}