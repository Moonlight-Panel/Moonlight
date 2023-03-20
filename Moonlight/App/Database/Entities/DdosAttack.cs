namespace Moonlight.App.Database.Entities;

public class DdosAttack
{
    public int Id { get; set; }
    public bool Ongoing { get; set; }
    public long Data { get; set; }
    public string Ip { get; set; } = "";
    public Node Node { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}