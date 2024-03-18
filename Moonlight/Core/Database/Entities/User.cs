namespace Moonlight.Core.Database.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public DateTime TokenValidTimestamp { get; set; } = DateTime.UtcNow.AddMinutes(-5);
    public bool Totp { get; set; } = false;
    public string TotpSecret { get; set; } = "";
    
    public int Permissions { get; set; } = 0;
    public string Flags { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}