using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Features.Users;

public class CreateUserDto
{
    [Required]
    [RegularExpression("^[a-z0-9]{2,}$", ErrorMessage = "Username must be at least 2 characters long and contain only lowercase letters and numbers")]
    [MaxLength(64)]
    public string Username { get; set; }
    
    [Required]
    [MaxLength(32)]
    public string DisplayName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }

    public bool AllowLocalAuth { get; set; }
    public string? Password { get; set; }
}