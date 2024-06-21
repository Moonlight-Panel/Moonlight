namespace Moonlight.Core.Database.Entities;

public class ApiKey
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;
    public string PermissionJson { get; set; } = "[]";
}