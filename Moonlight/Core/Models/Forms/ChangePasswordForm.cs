using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MoonCoreUI.Attributes;

namespace Moonlight.Core.Models.Forms;

public class ChangePasswordForm
{
    [Required(ErrorMessage = "You need to provide a password")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(256, ErrorMessage = "The password must not be longer than 256 characters")]
    [CustomFormType(Type = "password")]
    [Description("The new password you want to use.")]
    public string NewPassword { get; set; }
    
    [Required(ErrorMessage = "You need to provide a password")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(256, ErrorMessage = "The password must not be longer than 256 characters")]
    [CustomFormType(Type = "password")]
    [Description("Please repeat the new password.")]
    public string RepeatNewPassword { get; set; }
}