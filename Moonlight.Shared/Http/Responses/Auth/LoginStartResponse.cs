namespace Moonlight.Shared.Http.Responses.Auth;

public class LoginStartResponse
{
    public string ClientId { get; set; }
    public string Endpoint { get; set; }
    public string RedirectUri { get; set; }
}