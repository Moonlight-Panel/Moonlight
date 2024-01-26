using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms.Auth;

public class UpdateAccountPasswordForm
{
    [Required(ErrorMessage = "You need to specify a password")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(256, ErrorMessage = "The password must not be longer than 256 characters")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "You need to repeat your new password")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(256, ErrorMessage = "The password must not be longer than 256 characters")]
    public string RepeatedPassword { get; set; } = "";
}