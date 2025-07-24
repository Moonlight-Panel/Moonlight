using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Interfaces;
using Moonlight.Shared.Http.Responses.OAuth2;

namespace Moonlight.ApiServer.Implementations;

public class LocalOAuth2Provider : IOAuth2Provider
{
    private readonly AppConfiguration Configuration;
    private readonly ILogger<LocalOAuth2Provider> Logger;
    private readonly DatabaseRepository<User> UserRepository;

    public LocalOAuth2Provider(
        AppConfiguration configuration,
        ILogger<LocalOAuth2Provider> logger,
        DatabaseRepository<User> userRepository
    )
    {
        UserRepository = userRepository;
        Configuration = configuration;
        Logger = logger;
    }

    public Task<string> Start()
    {
        var redirectUri = string.IsNullOrEmpty(Configuration.Authentication.OAuth2.AuthorizationRedirect)
            ? Configuration.PublicUrl
            : Configuration.Authentication.OAuth2.AuthorizationRedirect;

        var endpoint = string.IsNullOrEmpty(Configuration.Authentication.OAuth2.AuthorizationEndpoint)
            ? Configuration.PublicUrl + "/oauth2/authorize"
            : Configuration.Authentication.OAuth2.AuthorizationEndpoint;

        var clientId = Configuration.Authentication.OAuth2.ClientId;
        
        var url = $"{endpoint}" +
                  $"?client_id={clientId}" +
                  $"&redirect_uri={redirectUri}" +
                  $"&response_type=code";

        return Task.FromResult(url);
    }

    public async Task<User?> Complete(string code)
    {
        // Create http client to call the auth provider
        var httpClient = new HttpClient();
        using var httpApiClient = new HttpApiClient(httpClient);

        httpClient.DefaultRequestHeaders.Add("Authorization",
            $"Basic {Configuration.Authentication.OAuth2.ClientSecret}");

        // Build access endpoint
        var accessEndpoint = string.IsNullOrEmpty(Configuration.Authentication.OAuth2.AccessEndpoint)
            ? $"{Configuration.PublicUrl}/oauth2/handle"
            : Configuration.Authentication.OAuth2.AccessEndpoint;

        // Build redirect uri
        var redirectUri = string.IsNullOrEmpty(Configuration.Authentication.OAuth2.AuthorizationRedirect)
            ? Configuration.PublicUrl
            : Configuration.Authentication.OAuth2.AuthorizationRedirect;

        // Call the auth provider
        OAuth2HandleResponse handleData;

        try
        {
            handleData = await httpApiClient.PostJson<OAuth2HandleResponse>(accessEndpoint, new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", Configuration.Authentication.OAuth2.ClientId)
                ]
            ));
        }
        catch (HttpApiException e)
        {
            if (e.Status == 400)
                Logger.LogTrace("The auth server returned an error: {e}", e);
            else
                Logger.LogCritical("The auth server returned an error: {e}", e);

            throw new HttpApiException("Unable to request user data", 500);
        }

        // Notice: We just look up the user id here
        // which works as our oauth2 provider is using the same db.
        // a real oauth2 provider would create a user here
        
        // Handle the returned data
        var userId = handleData.UserId;

        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == userId);

        return user;
    }
}