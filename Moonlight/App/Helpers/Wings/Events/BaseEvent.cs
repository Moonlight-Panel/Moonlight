namespace Moonlight.App.Helpers.Wings.Events;

public class BaseEvent
{
    public string Event { get; set; } = "";
    public string[] Args { get; set; } = Array.Empty<string>();
}