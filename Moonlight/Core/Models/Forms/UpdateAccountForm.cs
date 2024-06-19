using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms;

public class UpdateAccountForm
{
    [Required(ErrorMessage = "You need to provide an username")]
    [MinLength(6, ErrorMessage = "The username is too short")]
    [MaxLength(20, ErrorMessage = "The username cannot be longer than 20 characters")]
    [Description("The new username you want to use")]
    public string Username { get; set; } = "";
    
    [Required(ErrorMessage = "You need to provide an email address")]
    [EmailAddress(ErrorMessage = "You need to enter a valid email address")]
    [Description("The new username you want to use")]
    public string Email { get; set; } = "";
}