namespace Moonlight.App.Database.Entities;

public class Website
{
    public int Id { get; set; }
    public string BaseDomain { get; set; } = "";
    public int PleskId { get; set; }
    public PleskServer PleskServer { get; set; }
    public User Owner { get; set; }
    public string FtpLogin { get; set; } = "";
    public string FtpPassword { get; set; } = "";
}