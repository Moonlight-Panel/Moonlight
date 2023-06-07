namespace Moonlight.App.Helpers.Wings.Events;

public class SendTokenEvent
{
    public string Event { get; set; } = "auth";
    public List<string> Args = new();
}