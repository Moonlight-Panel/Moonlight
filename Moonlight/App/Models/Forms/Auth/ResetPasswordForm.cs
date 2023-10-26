using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms.Auth;

public class ResetPasswordForm
{
    [Required(ErrorMessage = "You need to specify an email address")]
    [EmailAddress(ErrorMessage = "You need to enter a valid email address")]
    public string Email { get; set; } = "";
}