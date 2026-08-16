namespace Moonlight.Api.Features.Auth;

public class AuthOptions
{
    public string Audience { get; set; }
    public string Authority { get; set; }
    public bool RequireHttpsMetadata { get; set; } = true;
}