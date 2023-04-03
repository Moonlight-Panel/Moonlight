using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms;

public class PasswordModel
{
    [Required(ErrorMessage = "You need to enter a password")]
    [MinLength(8, ErrorMessage = "You need to enter a password with minimum 8 characters in lenght")]
    public string Password { get; set; }
}