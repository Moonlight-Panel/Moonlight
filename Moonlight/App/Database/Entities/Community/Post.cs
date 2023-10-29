using Moonlight.App.Database.Enums;

namespace Moonlight.App.Database.Entities.Community;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public User Author { get; set; }
    public PostType Type { get; set; }
    public List<PostComment> Comments { get; set; } = new();
    public List<PostLike> Likes { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}