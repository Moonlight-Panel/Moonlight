namespace Moonlight.App.Database.Entities;

public class BlocklistIp
{
    public int Id { get; set; }
    public string Ip { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public long Packets { get; set; }
    public DateTime CreatedAt { get; set; }
}