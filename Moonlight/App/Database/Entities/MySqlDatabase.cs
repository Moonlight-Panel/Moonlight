namespace Moonlight.App.Database.Entities;

public class MySqlDatabase
{
    public int Id { get; set; }
    public WebSpace WebSpace { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}