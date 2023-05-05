using System.Text;
using Logging.Net;
using Moonlight.App.ApiClients.Google.Requests;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using RestSharp;

namespace Moonlight.App.Services.OAuth2;

public class GoogleOAuth2Service
{
    private readonly bool EnableGoogle;
    private readonly string GoogleClientId;
    private readonly string GoogleClientSecret;
    
    private readonly bool EnableOverrideUrl;
    private readonly string OverrideUrl;
    private readonly string AppUrl;

    public GoogleOAuth2Service(ConfigService configService)
    {
        var config = configService
            .GetSection("Moonlight")
            .GetSection("OAuth2");

        EnableGoogle = config
            .GetSection("Google")
            .GetValue<bool>("Enable");

        if (EnableGoogle)
        {
            GoogleClientId = config.GetSection("Google").GetValue<string>("ClientId");
            GoogleClientSecret = config.GetSection("Google").GetValue<string>("ClientSecret");
        }

        EnableOverrideUrl = config.GetValue<bool>("EnableOverrideUrl");

        if (EnableOverrideUrl)
            OverrideUrl = config.GetValue<string>("OverrideUrl");

        AppUrl = configService.GetSection("Moonlight").GetValue<string>("AppUrl");
    }
    
    public Task<string> GetUrl()
    {
        if (!EnableGoogle)
            throw new DisplayException("Google OAuth2 not enabled");

        var endpoint = GetBaseUrl() + "/api/moonlight/oauth2/google";
        var scope = "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email";

        return Task.FromResult(
            $"https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={GoogleClientId}&redirect_uri={endpoint}&scope={scope}"
        );
    }

    public async Task<User?> HandleCode(string code)
    {
        // Generate access token
        var endpoint = GetBaseUrl() + "/api/moonlight/oauth2/google";
        var googleEndpoint = "https://oauth2.googleapis.com/token";
        
        // Setup payload
        var payload = new GoogleOAuth2CodePayload()
        {
            Code = code,
            RedirectUri = endpoint,
            ClientId = GoogleClientId,
            ClientSecret = GoogleClientSecret
        };

        using var client = new RestClient();
        var request = new RestRequest(googleEndpoint);

        request.AddBody(payload);
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

        var googlePeopleEndpoint = "https://people.googleapis.com/v1/people/me";
        
        var getRequest = new RestRequest(googlePeopleEndpoint);
        getRequest.AddHeader("Authorization", $"Bearer {accessToken}");
        getRequest.AddParameter("personFields", "names,emailAddresses");

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

        return new()
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };
    }

    private string GetBaseUrl()
    {
        if (EnableOverrideUrl)
            return OverrideUrl;

        return AppUrl;
    }
}