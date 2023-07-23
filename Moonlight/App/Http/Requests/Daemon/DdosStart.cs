namespace Moonlight.App.Http.Requests.Daemon;

public class DdosStart
{
    public string Ip { get; set; } = "";
    public long Packets { get; set; }
}