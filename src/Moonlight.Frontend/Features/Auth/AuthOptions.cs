namespace Moonlight.Frontend.Features.Auth;

public class AuthOptions
{
    public string Authority { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public bool RequireHttpsMetadata { get; set; }
}