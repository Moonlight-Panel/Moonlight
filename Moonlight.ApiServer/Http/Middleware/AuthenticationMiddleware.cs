using System.Text.Json;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Extended.Models;
using MoonCore.Extended.OAuth2.ApiServer;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers.Authentication;

namespace Moonlight.ApiServer.Http.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate Next;
    private readonly ILogger<AuthenticationMiddleware> Logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        Next = next;
        Logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await Authenticate(context);
        await Next(context);
    }

    private async Task Authenticate(HttpContext context)
    {
        var request = context.Request;

        if (!request.Cookies.TryGetValue("ml-access", out var accessToken) ||
            !request.Cookies.TryGetValue("ml-refresh", out var refreshToken))
            return;
        
        if(string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            return;
        
        // TODO: Validate if both are valid jwts (maybe)
        
        //
        var tokenHelper = context.RequestServices.GetRequiredService<TokenHelper>();
        var configService = context.RequestServices.GetRequiredService<ConfigService<AppConfiguration>>();

        User? user = null;
        
        if (!await tokenHelper.IsValidAccessToken(accessToken, configService.Get().Authentication.MlAccessSecret,
                data =>
                {
                    if (!data.TryGetValue("userId", out var userIdStr))
                        return false;

                    if (!int.TryParse(userIdStr, out var userId))
                        return false;

                    var userRepo = context.RequestServices.GetRequiredService<DatabaseRepository<User>>();

                    user = userRepo.Get().FirstOrDefault(x => x.Id == userId);

                    return user != null;
                }))
        {
            return;
        }
        
        if(user == null)
            return;
        
        // Validate external access
        if (DateTime.UtcNow > user.RefreshTimestamp)
        {
            var tokenConsumer = new TokenConsumer(user.AccessToken, user.RefreshToken, user.RefreshTimestamp,
                async refreshToken =>
                {
                    var oauth2Service = context.RequestServices.GetRequiredService<OAuth2Service>();

                    var accessData = await oauth2Service.RefreshAccess(refreshToken);

                    user.AccessToken = accessData.AccessToken;
                    user.RefreshToken = accessData.RefreshToken;
                    user.RefreshTimestamp = DateTime.UtcNow.AddSeconds(accessData.ExpiresIn);

                    var userRepo = context.RequestServices.GetRequiredService<DatabaseRepository<User>>();
                
                    userRepo.Update(user);

                    return new TokenPair()
                    {
                        AccessToken = user.AccessToken,
                        RefreshToken = user.RefreshToken
                    };
                });

            await tokenConsumer.GetAccessToken();
            //TODO: API CALL
        }
        
        // Load permissions, handle empty values
        var permissions = JsonSerializer.Deserialize<string[]>(
            string.IsNullOrEmpty(user.PermissionsJson) ? "[]" : user.PermissionsJson
        ) ?? [];

        // Save permission state
        context.User = new PermClaimsPrinciple(permissions, user);

        /*
        string? token = null;

        // Cookie for Moonlight.Client
        if (request.Cookies.ContainsKey("token") && !string.IsNullOrEmpty(request.Cookies["token"]))
            token = request.Cookies["token"];

        // Header for api clients
        if (request.Headers.ContainsKey("Authorization") && !string.IsNullOrEmpty(request.Cookies["Authorization"]))
        {
            var headerValue = request.Cookies["Authorization"] ?? "";

            if (headerValue.StartsWith("Bearer"))
            {
                var headerParts = headerValue.Split(" ");

                if (headerParts.Length > 1 && !string.IsNullOrEmpty(headerParts[1]))
                    token = headerParts[1];
            }
        }

        if(token == null)
            return;

        // Validate token
        if (token.Length > 300)
        {
            Logger.LogWarning("Received token bigger than 300 characters, Length: {length}", token.Length);
            return;
        }

        // Decide which authentication method we need for the token
        if (token.Count(x => x == '.') == 2) // JWT only has two dots
            await AuthenticateUser(context, token);
        else
            await AuthenticateApiKey(context, token);*/
    }

    private async Task AuthenticateUser(HttpContext context, string jwt)
    {
        var jwtHelper = context.RequestServices.GetRequiredService<JwtHelper>();
        var configService = context.RequestServices.GetRequiredService<ConfigService<AppConfiguration>>();
        var secret = configService.Get().Authentication.Secret;

        if (!await jwtHelper.Validate(secret, jwt, "login"))
            return;

        var data = await jwtHelper.Decode(secret, jwt);

        if (!data.TryGetValue("iat", out var issuedAtString) || !data.TryGetValue("userId", out var userIdString))
            return;

        var userId = int.Parse(userIdString);
        var issuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(issuedAtString)).DateTime;

        var userRepo = context.RequestServices.GetRequiredService<DatabaseRepository<User>>();
        var user = userRepo.Get().FirstOrDefault(x => x.Id == userId);

        if (user == null)
            return;

        // Check if token is in the past
        if (user.TokenValidTimestamp > issuedAt)
            return;

        // Load permissions, handle empty values
        var permissions = JsonSerializer.Deserialize<string[]>(
            string.IsNullOrEmpty(user.PermissionsJson) ? "[]" : user.PermissionsJson
        ) ?? [];

        // Save permission state
        context.User = new PermClaimsPrinciple(permissions, user);
    }

    private async Task AuthenticateApiKey(HttpContext context, string apiKey)
    {
    }
}