using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms;

public class UserRegisterModel
{
    [Required, EmailAddress]
    public string Email { get; set; }
    
    [Required, MinLength(3)]
    public string FirstName { get; set; }
    
    [Required, MinLength(3)]
    public string LastName { get; set; }
    
    [Required]
    public string Password { get; set; }
    
    [Required]
    public string ConfirmPassword { get; set; }
}