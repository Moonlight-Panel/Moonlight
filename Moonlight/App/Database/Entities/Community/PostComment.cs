namespace Moonlight.App.Database.Entities.Community;

public class PostComment
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public User Author { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}