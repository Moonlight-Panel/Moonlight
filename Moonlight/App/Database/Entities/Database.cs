namespace Moonlight.App.Database.Entities;

public class Database
{
    public int Id { get; set; }
    public int AaPanelId { get; set; }
    public User Owner { get; set; }
}