using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "You need to provide an email address")]
    [EmailAddress(ErrorMessage = "You need to provide a valid email address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "You need to provide a password")]
    public string Password { get; set; }
}