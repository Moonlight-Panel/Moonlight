using Moonlight.Core.Database.Entities;

namespace Moonlight.Core.Models.Abstractions;

public class Session
{
    public string Ip { get; set; } = "N/A";
    public string Url { get; set; } = "N/A";
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // To remove inactive sessions
}