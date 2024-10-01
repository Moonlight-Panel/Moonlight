namespace Moonlight.Shared.Http.Responses.Auth;

public class CheckResponse
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string[] Permissions { get; set; }
}