using Microsoft.AspNetCore.Authentication;

namespace Moonlight.ApiServer.Implementations.LocalAuth;

public class LocalAuthOptions : AuthenticationSchemeOptions
{
    public string? SignInScheme { get; set; }
}