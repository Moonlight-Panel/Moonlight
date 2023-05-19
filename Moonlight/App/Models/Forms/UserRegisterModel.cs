using System.ComponentModel.DataAnnotations;
using Moonlight.App.Helpers;

namespace Moonlight.App.Models.Forms;

public class UserRegisterModel
{
    [Required(ErrorMessage = "You need to specify a email address")]
    [EmailAddress(ErrorMessage = "You need to specify a valid email address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "You need to specify a first name")]
    [MinLength(3, ErrorMessage = "You need to specify a valid first name")]
    [MaxLength(30, ErrorMessage = "You need to specify a valid first name")]
    public string FirstName { get; set; }
    
    
    [Required(ErrorMessage = "You need to specify a last name")]
    [MinLength(3, ErrorMessage = "You need to specify a valid last name")]
    [MaxLength(30, ErrorMessage = "You need to specify a valid last name")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "You need to specify a password")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "You need to specify a password")]
    public string ConfirmPassword { get; set; }
    
    [MustBeTrue(ErrorMessage = "Please solve the captcha")]
    public bool Captcha { get; set; }
}