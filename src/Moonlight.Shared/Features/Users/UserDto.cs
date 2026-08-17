namespace Moonlight.Shared.Features.Users;

public class UserDto
{
    public int Id { get; set; }
    
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string? Email { get; set; }

    public bool AllowLocalAuth { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}