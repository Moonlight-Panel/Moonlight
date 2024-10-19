namespace Moonlight.Shared.Http.Responses.Auth;

public class RefreshResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}