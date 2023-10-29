namespace Moonlight.App.Database.Entities.Community;

public class PostLike
{
    public int Id { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}