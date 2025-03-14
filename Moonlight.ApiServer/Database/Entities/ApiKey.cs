using System.ComponentModel.DataAnnotations.Schema;

namespace Moonlight.ApiServer.Database.Entities;

public class ApiKey
{
    public int Id { get; set; }

    public string Description { get; set; }
    
    [Column(TypeName="jsonb")]
    public string PermissionsJson { get; set; } = "[]";
    
    [Column(TypeName = "timestamp with time zone")]
    public DateTime ExpiresAt { get; set; }
    
    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}