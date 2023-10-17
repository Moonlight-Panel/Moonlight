using Moonlight.App.Database.Enums;

namespace Moonlight.App.Database.Entities.Tickets;

public class Ticket
{
    public int Id { get; set; }
    public User Creator { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Tries { get; set; } = "";
    public TicketPriority Priority { get; set; } = TicketPriority.Low;
    public bool Open { get; set; } = true;

    public List<TicketMessage> Messages = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}