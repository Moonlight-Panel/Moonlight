using MoonCore.Extended.OAuth2.Consumer;

namespace Moonlight.ApiServer.Database.Entities;

public class User : IUserModel
{
    public int Id { get; set; }

    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public DateTime TokenValidTimestamp { get; set; } = DateTime.UtcNow;
    public string PermissionsJson { get; set; } = "[]";

    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public DateTime RefreshTimestamp { get; set; } = DateTime.UtcNow;
}