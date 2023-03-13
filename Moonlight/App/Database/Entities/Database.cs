namespace Moonlight.App.Database.Entities;

public class Database
{
    public int Id { get; set; }
    public int InternalAaPanelId { get; set; }
    public User Owner { get; set; }
    public AaPanel AaPanel { get; set; }
    public string Name { get; set; }
}