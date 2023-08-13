namespace Moonlight.App.Http.Requests.Daemon;

public class DdosStop
{
    public string Ip { get; set; } = "";
    public long Traffic { get; set; }
}