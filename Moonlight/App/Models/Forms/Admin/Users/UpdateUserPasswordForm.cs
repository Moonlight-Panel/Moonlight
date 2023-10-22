using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms.Admin.Users;

public class UpdateUserPasswordForm
{
    [Required(ErrorMessage = "You need to specify a password")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(256, ErrorMessage = "The password must not be longer than 256 characters")]
    public string Password { get; set; } = "";
}