namespace Moonlight.Shared.Http.Responses.Auth;

public class OAuth2StartResponse
{
    public string Endpoint { get; set; }
    public string ClientId { get; set; }
    public string RedirectUri { get; set; }
}