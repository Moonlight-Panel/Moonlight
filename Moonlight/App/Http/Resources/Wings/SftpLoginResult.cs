namespace Moonlight.App.Http.Resources.Wings;

public class SftpLoginResult
{
    public string Server { get; set; }
    public string User { get; set; }
    public List<string> Permissions { get; set; }
}