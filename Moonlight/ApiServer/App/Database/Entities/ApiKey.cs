namespace Moonlight.ApiServer.App.Database.Entities;

public class ApiKey
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    public string? Description { get; set; } = "";
    public string PermissionsJson { get; set; } = "[]";

    public string Key { get; set; }
    
    public DateTime ExpireDate { get; set; }
}