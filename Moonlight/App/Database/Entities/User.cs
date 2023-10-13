namespace Moonlight.App.Database.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Avatar { get; set; } = null;
    public string? TotpKey { get; set; } = null;
    
    // Meta data
    public string Flags { get; set; } = "";
    public int Permissions { get; set; } = 0;
    
    // Timestamps
    public DateTime TokenValidTimestamp { get; set; } = DateTime.UtcNow.AddMinutes(-10);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}