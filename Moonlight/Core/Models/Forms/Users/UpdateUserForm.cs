using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms.Users;

public class UpdateUserForm
{
    [Required(ErrorMessage = "You need to provide an username")]
    [MinLength(6, ErrorMessage = "The username is too short")]
    [MaxLength(20, ErrorMessage = "The username cannot be longer than 20 characters")]
    [RegularExpression("^[a-z][a-z0-9]*$", ErrorMessage = "Usernames can only contain lowercase characters and numbers and should not start with a number")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "You need to provide an email address")]
    [EmailAddress(ErrorMessage = "You need to enter a valid email address")]
    public string Email { get; set; }
    
    [Description("This toggles the use of the two factor authentication")]
    //TODO: [RadioButtonBool("Enabled", "Disabled", TrueIcon = "bx-lock-alt", FalseIcon = "bx-lock-open-alt")]
    [DisplayName("Two factor authentication")]
    public bool Totp { get; set; } = false;
}