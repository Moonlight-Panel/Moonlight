using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms;

public class LoginForm
{
    [Required(ErrorMessage = "You need to provide an email address")]
    [EmailAddress(ErrorMessage = "You need to enter a valid email address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "You need to provide a password")]
    public string Password { get; set; }

    public string TwoFactorCode { get; set; } = "";
}