using System.Text;
using Logging.Net;
using Moonlight.App.ApiClients.Google.Requests;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Moonlight.App.Services;
using RestSharp;

namespace Moonlight.App.OAuth2.Providers;

public class GoogleOAuth2Provider : OAuth2Provider
{
    public override Task<string> GetUrl()
    {
        var endpoint = Url + "/api/moonlight/oauth2/google";
        var scope = "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email";

        return Task.FromResult(
            $"https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={Config.ClientId}&redirect_uri={endpoint}&scope={scope}"
        );
    }

    public override async Task<User> HandleCode(string code)
    {
        // Endpoints
        var endpoint = Url + "/api/moonlight/oauth2/google";
        var googleEndpoint = "https://oauth2.googleapis.com/token";
        var googlePeopleEndpoint = "https://people.googleapis.com/v1/people/me";

        // Generate access token
        
        // Setup payload
        var payload = new GoogleOAuth2CodePayload()
        {
            Code = code,
            RedirectUri = endpoint,
            ClientId = Config.ClientId,
            ClientSecret = Config.ClientSecret
        };

        using var client = new RestClient();
        var request = new RestRequest(googleEndpoint);

        request.AddBody(payload);
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
        
        // Now, we will call the google api with our access token to get the data we need
        
        var getRequest = new RestRequest(googlePeopleEndpoint);
        getRequest.AddHeader("Authorization", $"Bearer {accessToken}");
        getRequest.AddParameter("personFields", "names,emailAddresses");

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

        var firstName = getData
            .GetSection("names")
            .GetChildren()
            .First()
            .GetValue<string>("givenName");
        
        var lastName = getData
            .GetSection("names")
            .GetChildren()
            .First()
            .GetValue<string>("familyName");
        
        var email = getData
            .GetSection("emailAddresses")
            .GetChildren()
            .First()
            .GetValue<string>("value");

        using var scope = ServiceScopeFactory.CreateScope();
        
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        
        if (userRepo.Get().Any(x => x.Email == email))
        {
            var user = userRepo.Get().First(x => x.Email == email);
            
            return user;
        }
        else
        {
            await userService.Register(
                email,
                StringHelper.GenerateString(32),
                firstName,
                lastName
            );

            var user = userRepo.Get().First(x => x.Email == email);

            return user;
        }
    }
}