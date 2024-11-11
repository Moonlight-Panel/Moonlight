using System.Text.Json;
using MoonCore.Authentication;
using MoonCore.Extended.Abstractions;
using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Http.Middleware;

public class ApiAuthenticationMiddleware
{
    private readonly RequestDelegate Next;
    private readonly ILogger<ApiAuthenticationMiddleware> Logger;

    public ApiAuthenticationMiddleware(RequestDelegate next, ILogger<ApiAuthenticationMiddleware> logger)
    {
        Next = next;
        Logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await Authenticate(context);
        await Next(context);
    }

    public Task Authenticate(HttpContext context)
    {
        var request = context.Request;
        
        if(!request.Headers.ContainsKey("Authorization"))
            return Task.CompletedTask;
        
        if(request.Headers["Authorization"].Count == 0)
            return Task.CompletedTask;

        var authHeader = request.Headers["Authorization"].First();
        
        if(string.IsNullOrEmpty(authHeader))
            return Task.CompletedTask;

        var parts = authHeader.Split(" ");
        
        if(parts.Length != 2)
            return Task.CompletedTask;

        var bearerValue = parts[1];
        
        if(!bearerValue.StartsWith("api_"))
            return Task.CompletedTask;
        
        if(bearerValue.Length != "api_".Length + 32)
            return Task.CompletedTask;

        var apiKeyRepo = context.RequestServices.GetRequiredService<DatabaseRepository<ApiKey>>();
        var apiKey = apiKeyRepo.Get().FirstOrDefault(x => x.Secret == bearerValue);
        
        if(apiKey == null)
            return Task.CompletedTask;

        var permissions = JsonSerializer.Deserialize<string[]>(apiKey.PermissionsJson) ?? [];
        context.User = new PermClaimsPrinciple()
        {
            Permissions = permissions
        };
        
        return Task.CompletedTask;
    }
}