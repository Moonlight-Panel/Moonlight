namespace Moonlight.ApiServer.Database.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public DateTime TokenValidTimestamp { get; set; } = DateTime.UtcNow;
    public string PermissionsJson { get; set; } = "[]";
}