namespace Moonlight.App.Models.Misc;

public class LogEntry
{
    public string Level { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}