using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms;

public class ChangePasswordForm
{
    [Required(ErrorMessage = "You need to provide a password")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(256, ErrorMessage = "The password must not be longer than 256 characters")]
    //TODO: [CustomFormType(Type = "password")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "You need to provide a password")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(256, ErrorMessage = "The password must not be longer than 256 characters")]
    //TODO: [CustomFormType(Type = "password")]
    public string RepeatedPassword { get; set; }
}