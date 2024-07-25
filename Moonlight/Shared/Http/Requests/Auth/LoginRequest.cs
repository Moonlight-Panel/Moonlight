using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Auth;

public class LoginRequest
{
    public string Identifier { get; set; }
    
    [Required(ErrorMessage = "You need to provide a password")]
    public string Password { get; set; }

    public string? TwoFactorCode { get; set; }
}