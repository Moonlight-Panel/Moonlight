namespace Moonlight.App.Database.Entities.LogsEntries;

public class ErrorLogEntry
{
    public int Id { get; set; }
    public string Stacktrace { get; set; } = "";
    public bool System { get; set; }
    public string JsonData { get; set; } = "";
    public string Ip { get; set; } = "";
    public string Class { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}