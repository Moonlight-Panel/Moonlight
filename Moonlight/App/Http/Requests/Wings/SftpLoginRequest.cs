namespace Moonlight.App.Http.Requests.Wings;

public class SftpLoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Ip { get; set; }
    public string Type { get; set; }
}