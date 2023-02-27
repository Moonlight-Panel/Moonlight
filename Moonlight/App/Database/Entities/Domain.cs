namespace Moonlight.App.Database.Entities;

public class Domain
{
    public int Id { get; set; }
    public string Name { get; set; }
    public SharedDomain SharedDomain { get; set; }
    public User Owner { get; set; }
}