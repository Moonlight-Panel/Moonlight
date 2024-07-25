namespace Moonlight.Shared.Http.Resources.Admin.Users;

public class DetailUserResponse
{
    public int Id { get; set; }

    public string Username { get; set; }
    public string Email { get; set; }
    public string PermissionsJson { get; set; } = "[]";

    public DateTime TokenValidTime { get; set; } = DateTime.UtcNow;
}