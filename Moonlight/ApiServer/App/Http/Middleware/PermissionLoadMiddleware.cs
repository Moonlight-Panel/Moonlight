using System.Text.Json;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Exceptions;
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

        if (headerValue.StartsWith("api_"))
            await HandleApiKey(headerValue, context);
        else
            await HandleUser(headerValue, context);
    }

    private async Task HandleUser(string authHeaderValue, HttpContext context)
    {
        var jwtHelper = context.RequestServices.GetRequiredService<JwtHelper>();
        var configService = context.RequestServices.GetRequiredService<ConfigService<AppConfiguration>>();
        var secret = configService.Get().Security.Token;
        
        if(!await jwtHelper.Validate(secret, authHeaderValue, "userLogin"))
            return;

        var jwtData = await jwtHelper.Decode(secret, authHeaderValue);
        
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
        ).ToArray(); //TODO: Find a better way to do that. This looks kinda inefficient
        
        context.SetPermissions(finalPermissions);
    }
    
    private Task HandleApiKey(string authHeaderValue, HttpContext context)
    {
        // This check prevents unfiltered database searches for a specific api key which could led to a DOS attack vector
        if (authHeaderValue.Length > 40)
            return Task.CompletedTask; // throw new ApiException("API Key cannot be longer than 32 characters", statusCode: 413);
        
        var apiKeyRepo = context.RequestServices.GetRequiredService<DatabaseRepository<ApiKey>>();
        var apiKey = apiKeyRepo.Get().FirstOrDefault(x => x.Key == authHeaderValue);
        
        if(apiKey == null)
            return Task.CompletedTask;

        if (apiKey.ExpireDate < DateTime.UtcNow)
            throw new ApiException($"This api key is expired at {Formatter.FormatDate(apiKey.ExpireDate)}", statusCode: 401);
        
        // Load permissions
        var permissionsJson = string.IsNullOrEmpty(apiKey.PermissionsJson) ? "[]" : apiKey.PermissionsJson;
        var permissions = JsonSerializer.Deserialize<string[]>(permissionsJson) ?? [];
        
        context.SetPermissions(permissions);
        return Task.CompletedTask;
    }
}