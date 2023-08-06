namespace Moonlight.App.Database.Entities;

public class TicketMessage
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public string? AttachmentUrl { get; set; }
    public User? Sender { get; set; }
    public bool IsSystemMessage { get; set; }
    public bool IsEdited { get; set; }
    public bool IsSupportMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}