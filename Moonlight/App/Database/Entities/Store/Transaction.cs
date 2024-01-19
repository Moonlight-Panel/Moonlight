namespace Moonlight.App.Database.Entities.Store;

public class Transaction
{
    public int Id { get; set; }
    public double Price { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}