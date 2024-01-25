using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms.Services;

public class AddUserToServiceForm
{
    [Required(ErrorMessage = "You need to provide an username")]
    [MinLength(7, ErrorMessage = "The username is too short")]
    [MaxLength(20, ErrorMessage = "The username cannot be longer than 20 characters")]
    [RegularExpression("^[a-z][a-z0-9]*$",
        ErrorMessage = "Usernames can only contain lowercase characters and numbers")]
    public string Username { get; set; } = "";
}