using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Admin.Users;

public class CreateUserRequest
{
    [Required(ErrorMessage = "You need to provide an email address")]
    [EmailAddress(ErrorMessage = "You need to provide a valid email address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "You need to provide a username")]
    [RegularExpression("^[a-z][a-z0-9]*$", ErrorMessage = "Usernames can only contain lowercase characters and numbers and should not start with a number")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "You need to provide a password")]
    [MinLength(8, ErrorMessage = "Your password needs to be at least 8 characters long")]
    [MaxLength(256, ErrorMessage = "Your password should not exceed the length of 256 characters")]
    public string Password { get; set; }
    
    public string PermissionsJson { get; set; } = "[]";
}