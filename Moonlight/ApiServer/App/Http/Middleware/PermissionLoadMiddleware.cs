using System.Text.Json;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Extensions;
using Moonlight.ApiServer.App.Interfaces;
using Moonlight.Shared.Models;

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
        
        // UserId
        if(!jwtData.TryGetValue("UserId", out var userIdText))
            return;
        
        if(!int.TryParse(userIdText, out var userId))
            return;
        
        // Issued At
        if(!jwtData.TryGetValue("iat", out var issuedAtText))
            return;
        
        if(!int.TryParse(issuedAtText, out var issuedAtTimestamp))
            return;

        // Load user data
        var userRepo = context.RequestServices.GetRequiredService<DatabaseRepository<User>>();
        var user = userRepo.Get().FirstOrDefault(x => x.Id == userId);
        
        if(user == null)
            return;
        
        // Token valid timestamp
        var provider = context.RequestServices.GetRequiredService<IAuthenticationProvider>();
        var tokenValidTime = await provider.GetTokenValidTimestamp(context.RequestServices, user.Id);
        var issuedAt = DateTimeOffset.FromUnixTimeSeconds(issuedAtTimestamp).DateTime;
        
        // Check if the token is in the past compared to the timestamp after which all tokens become valid
        if(tokenValidTime > issuedAt)
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