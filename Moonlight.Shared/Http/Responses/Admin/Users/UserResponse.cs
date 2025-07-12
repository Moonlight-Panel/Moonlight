namespace Moonlight.Shared.Http.Responses.Admin.Users;

public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string[] Permissions { get; set; }
}