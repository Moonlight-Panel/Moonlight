using Moonlight.Core.Database.Entities;

namespace Moonlight.Features.Community.Entities;

public class PostComment
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public User Author { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}