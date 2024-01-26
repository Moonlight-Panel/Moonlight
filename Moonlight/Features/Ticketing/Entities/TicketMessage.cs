using Moonlight.Core.Database.Entities;

namespace Moonlight.Features.Ticketing.Entities;

public class TicketMessage
{
    public int Id { get; set; }
    public User? Sender { get; set; }
    public bool IsSupport { get; set; }
    public string Content { get; set; } = "";
    public string? Attachment { get; set; }
    public DateTime CreatedAt { get; set; }
}