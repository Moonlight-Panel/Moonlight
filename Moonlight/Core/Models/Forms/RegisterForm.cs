using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms;

public class RegisterForm
{
    [Required(ErrorMessage = "You need to provide an username")]
    [MinLength(6, ErrorMessage = "The username is too short")]
    [MaxLength(20, ErrorMessage = "The username cannot be longer than 20 characters")]
    [RegularExpression("^[a-z][a-z0-9]*$", ErrorMessage = "Usernames can only contain lowercase characters and numbers and should not start with a number")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "You need to provide an email address")]
    [EmailAddress(ErrorMessage = "You need to enter a valid email address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "You need to provide a password")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(256, ErrorMessage = "The password must not be longer than 256 characters")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "You need to provide a password")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(256, ErrorMessage = "The password must not be longer than 256 characters")]
    public string RepeatedPassword { get; set; }
}