namespace Moonlight.App.Database.Entities;

public class WebSpace
{
    public int Id { get; set; }
    public string Domain { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string VHostTemplate { get; set; } = "";
    public User Owner { get; set; }
    public List<MySqlDatabase> Databases { get; set; } = new();
    public CloudPanel CloudPanel { get; set; }
}