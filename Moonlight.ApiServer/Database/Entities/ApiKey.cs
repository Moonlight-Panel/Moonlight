using System.ComponentModel.DataAnnotations.Schema;

namespace Moonlight.ApiServer.Database.Entities;

public class ApiKey
{
    public int Id { get; set; }

    public string Description { get; set; }
    
    public string[] Permissions { get; set; } = [];
    
    public DateTimeOffset ExpiresAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}