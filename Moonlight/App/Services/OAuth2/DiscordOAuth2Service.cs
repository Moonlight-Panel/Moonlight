using System.Text;
using Logging.Net;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Models.Google.Requests;
using Moonlight.App.Models.Misc;
using RestSharp;

namespace Moonlight.App.Services.OAuth2;

public class DiscordOAuth2Service
{
    private readonly bool Enable;
    private readonly string ClientId;
    private readonly string ClientSecret;
    
    private readonly bool EnableOverrideUrl;
    private readonly string OverrideUrl;
    private readonly string AppUrl;

    public DiscordOAuth2Service(ConfigService configService)
    {
        var config = configService
            .GetSection("Moonlight")
            .GetSection("OAuth2");

        Enable = config
            .GetSection("Discord")
            .GetValue<bool>("Enable");

        if (Enable)
        {
            ClientId = config.GetSection("Discord").GetValue<string>("ClientId");
            ClientSecret = config.GetSection("Discord").GetValue<string>("ClientSecret");
        }

        EnableOverrideUrl = config.GetValue<bool>("EnableOverrideUrl");

        if (EnableOverrideUrl)
            OverrideUrl = config.GetValue<string>("OverrideUrl");

        AppUrl = configService.GetSection("Moonlight").GetValue<string>("AppUrl");
    }
    
    public Task<string> GetUrl()
    {
        if (!Enable)
            throw new DisplayException("Discord OAuth2 not enabled");
        
        string url = $"https://discord.com/api/oauth2/authorize?client_id={ClientId}" +
            $"&redirect_uri={GetBaseUrl()}/api/moonlight/oauth2/discord" +
            "&response_type=code&scope=identify%20email";
        
        return Task.FromResult(
            url
        );
    }

    public async Task<User?> HandleCode(string code)
    {
        // Generate access token
        var endpoint = GetBaseUrl() + "/api/moonlight/oauth2/discord";
        var discordEndpoint = "https://discordapp.com/api/oauth2/token";

        using var client = new RestClient();
        var request = new RestRequest(discordEndpoint);

        request.AddParameter("client_id", ClientId);
        request.AddParameter("client_secret", ClientSecret);
        request.AddParameter("grant_type", "authorization_code");
        request.AddParameter("code", code);
        request.AddParameter("redirect_uri", endpoint);
        
        var response = await client.ExecutePostAsync(request);

        if (!response.IsSuccessful)
        {
            //TODO: Maybe add better error handling
            Logger.Debug("oAuth2 validate error: " + response.Content!);
            return null;
        }
        
        // parse response
        
        var data = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(response.Content!))
        ).Build();

        var accessToken = data.GetValue<string>("access_token");
        
        // Now, we will call the google api with our access token to get the data we need

        var googlePeopleEndpoint = "https://discordapp.com/api/users/@me";
        
        var getRequest = new RestRequest(googlePeopleEndpoint);
        getRequest.AddHeader("Authorization", $"Bearer {accessToken}");

        var getResponse = await client.ExecuteGetAsync(getRequest);
        
        if (!getResponse.IsSuccessful)
        {
            //TODO: Maybe add better error handling
            Logger.Debug("OAuth2 api access error: " + getResponse.Content!);
            return null;
        }
        
        // Parse response
        
        var getData = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(getResponse.Content!))
        ).Build();

        return new User()
        {
            Email = getData.GetValue<string>("email"),
            FirstName = "User",
            LastName = "User",
            DiscordId = getData.GetValue<long>("id"),
            Status = UserStatus.DataPending
        };
    }

    private string GetBaseUrl()
    {
        if (EnableOverrideUrl)
            return OverrideUrl;

        return AppUrl;
    }
}