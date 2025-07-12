using System.ComponentModel.DataAnnotations.Schema;

namespace Moonlight.ApiServer.Database.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTimeOffset TokenValidTimestamp { get; set; } = DateTimeOffset.MinValue;
    public string[] Permissions { get; set; } = [];
}