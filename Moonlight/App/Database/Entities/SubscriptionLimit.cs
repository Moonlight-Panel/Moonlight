namespace Moonlight.App.Database.Entities;

public class SubscriptionLimit
{
    public int Id { get; set; }
    public Image Image { get; set; } = null!;
    public int Amount { get; set; }
    public int Cpu { get; set; }
    public int Memory { get; set; }
    public int Disk { get; set; }
}