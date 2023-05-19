namespace Moonlight.App.Database.Entities;

public class IpBan
{
    public int Id { get; set; }
    public string Ip { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}