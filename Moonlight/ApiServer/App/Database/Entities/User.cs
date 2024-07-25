namespace Moonlight.ApiServer.App.Database.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PermissionsJson { get; set; } = "[]";

    public DateTime TokenValidTime { get; set; } = DateTime.UtcNow;
}