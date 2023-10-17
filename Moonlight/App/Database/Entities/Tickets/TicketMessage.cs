namespace Moonlight.App.Database.Entities.Tickets;

public class TicketMessage
{
    public int Id { get; set; }
    public User? Sender { get; set; }
    public bool IsSupport { get; set; }
    public string Content { get; set; } = "";
    public string? Attachment { get; set; }
    public DateTime CreatedAt { get; set; }
}