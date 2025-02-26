using System.ComponentModel.DataAnnotations.Schema;

namespace Moonlight.ApiServer.Database.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    
    [Column(TypeName="timestamp with time zone")]
    public DateTime TokenValidTimestamp { get; set; } = DateTime.MinValue;
    
    [Column(TypeName="jsonb")]
    public string PermissionsJson { get; set; } = "[]";
}