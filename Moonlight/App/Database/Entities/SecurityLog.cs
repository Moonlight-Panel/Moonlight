namespace Moonlight.App.Database.Entities;

public class SecurityLog
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}