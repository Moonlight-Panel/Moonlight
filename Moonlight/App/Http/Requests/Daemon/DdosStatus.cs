namespace Moonlight.App.Http.Requests.Daemon;

public class DdosStatus
{
    public bool Ongoing { get; set; }
    public long Data { get; set; }
    public string Ip { get; set; } = "";
}