using System.Text;
using Logging.Net;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Moonlight.App.Services;
using RestSharp;

namespace Moonlight.App.OAuth2.Providers;

public class DiscordOAuth2Provider : OAuth2Provider
{
    public override Task<string> GetUrl()
    {
        string url = $"https://discord.com/api/oauth2/authorize?client_id={Config.ClientId}" +
                     $"&redirect_uri={Url}/api/moonlight/oauth2/discord" +
                     "&response_type=code&scope=identify%20email";
        
        return Task.FromResult(
            url
        );
    }

    public override async Task<User> HandleCode(string code)
    {
        // Endpoints
        
        var endpoint = Url + "/api/moonlight/oauth2/discord";
        var discordUserDataEndpoint = "https://discordapp.com/api/users/@me";
        var discordEndpoint = "https://discordapp.com/api/oauth2/token";

        // Generate access token
        
        using var client = new RestClient();
        var request = new RestRequest(discordEndpoint);

        request.AddParameter("client_id", Config.ClientId);
        request.AddParameter("client_secret", Config.ClientSecret);
        request.AddParameter("grant_type", "authorization_code");
        request.AddParameter("code", code);
        request.AddParameter("redirect_uri", endpoint);
        
        var response = await client.ExecutePostAsync(request);

        if (!response.IsSuccessful)
        {
            Logger.Warn("Error verifying oauth2 code");
            Logger.Warn(response.ErrorMessage);
            throw new DisplayException("An error occured while verifying oauth2 code");
        }
        
        // parse response
        
        var data = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(response.Content!))
        ).Build();

        var accessToken = data.GetValue<string>("access_token");
        
        // Now, we will call the discord api with our access token to get the data we need

        var getRequest = new RestRequest(discordUserDataEndpoint);
        getRequest.AddHeader("Authorization", $"Bearer {accessToken}");

        var getResponse = await client.ExecuteGetAsync(getRequest);
        
        if (!getResponse.IsSuccessful)
        {
            Logger.Warn("An unexpected error occured while fetching user data from remote api");
            Logger.Warn(getResponse.ErrorMessage);
            
            throw new DisplayException("An unexpected error occured while fetching user data from remote api");
        }
        
        // Parse response
        
        var getData = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(getResponse.Content!))
        ).Build();

        var email = getData.GetValue<string>("email");
        var id = getData.GetValue<ulong>("id");
        
        // Handle data
        
        using var scope = ServiceScopeFactory.CreateScope();
        
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();

        if (userRepo.Get().Any(x => x.Email == email))
        {
            var user = userRepo.Get().First(x => x.Email == email);

            user.DiscordId = id;
            
            userRepo.Update(user);

            return user;
        }
        else
        {
            await userService.Register(
                email,
                StringHelper.GenerateString(32),
                "User",
                "User"
            );

            var user = userRepo.Get().First(x => x.Email == email);
            user.Status = UserStatus.DataPending;

            user.DiscordId = id;
            
            userRepo.Update(user);

            return user;
        }
    }
}