namespace Moonlight.App.Database.Entities;

public class Website
{
    public int Id { get; set; }
    public int InternalAaPanelId { get; set; }
    public AaPanel AaPanel { get; set; }
    public User Owner { get; set; }
    public string DomainName { get; set; }
    public string PhpVersion { get; set; }
    public string FtpUsername { get; set; }
    public string FtpPassword { get; set; }
}