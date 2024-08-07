using MoonCore.Attributes;

namespace Moonlight.Client.App.Services;

[Scoped]
public class IdentityService
{
    public bool IsLoggedIn { get; private set; }
    public string Token { get; private set; } = "";

    public readonly HttpClient Http;
    
    public IdentityService(HttpClient http)
    {
        Http = http;
    }

    public void SetLoginState(bool newState)
    {
        IsLoggedIn = newState;
    }

    public void SetToken(string token)
    {
        Token = token;

        if (Http.DefaultRequestHeaders.Contains("Authorization"))
            Http.DefaultRequestHeaders.Remove("Authorization");
        
        if(string.IsNullOrEmpty(token))
            return;
        
        Http.DefaultRequestHeaders.Add("Authorization", token);
    }
}