namespace Moonlight.Features.Servers.Http.Requests;

public class FtpLogin
{
    public string Username { get; set; }
    public string Password { get; set; }
    public int ServerId { get; set; }
}