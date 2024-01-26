using Moonlight.Core.Database.Entities;

namespace Moonlight.Features.Community.Entities;

public class PostLike
{
    public int Id { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}