namespace Moonlight.App.Database.Entities;

public class Website
{
    public int Id { get; set; }
    public int PleskId { get; set; }
    public User Owner { get; set; }
    public PleskServer PleskServer { get; set; }
    public string BaseDomain { get; set; }
}