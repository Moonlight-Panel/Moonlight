namespace Moonlight.Shared.Http.Resources.Auth;

public class CheckResponse
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string[] Permissions { get; set; }
}