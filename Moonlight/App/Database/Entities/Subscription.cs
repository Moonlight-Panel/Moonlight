namespace Moonlight.App.Database.Entities;

public class Subscription
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string SellPassId { get; set; } = "";
    public int Duration { get; set; }
    public List<SubscriptionLimit> Limits { get; set; } = new();
}