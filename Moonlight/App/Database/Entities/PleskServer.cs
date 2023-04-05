namespace Moonlight.App.Database.Entities;

public class PleskServer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ApiUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
}