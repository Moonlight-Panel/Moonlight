namespace Moonlight.Shared.Http.Requests.Admin.Users;

public class CreateUserRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PermissionsJson { get; set; } = "[]";

    public DateTime TokenValidTime { get; set; } = DateTime.UtcNow;
}