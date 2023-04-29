namespace Moonlight.App.MessageSystem;

public class MessageSubscriber
{
    public string Name { get; set; }
    public object Action { get; set; }
    public Type Type { get; set; }
    public object Bind { get; set; }
}