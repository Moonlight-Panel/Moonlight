using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Misc;

public class LoginDataModel
{
    [Required(ErrorMessage = "You need to enter an email address")]
    [EmailAddress(ErrorMessage = "You need to enter a valid email address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "You need to enter a password")]
    [MinLength(8, ErrorMessage = "You need to enter a password with minimum 8 characters in lenght")]
    public string Password { get; set; }
}