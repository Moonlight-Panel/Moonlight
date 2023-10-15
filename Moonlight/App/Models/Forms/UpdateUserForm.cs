using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms;

public class UpdateUserForm
{
    [Required(ErrorMessage = "You need to enter a username")]
    [MinLength(7, ErrorMessage = "The username is too short")]
    [MaxLength(20, ErrorMessage = "The username cannot be longer than 20 characters")]
    [RegularExpression("^[a-z][a-z0-9]*$", ErrorMessage = "Usernames can only contain lowercase characters and numbers")]
    public string Username { get; set; } = "";
    
    [Required(ErrorMessage = "You need to enter a email address")]
    [EmailAddress(ErrorMessage = "You need to enter a valid email address")]
    public string Email { get; set; } = "";
}