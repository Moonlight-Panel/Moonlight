using Moonlight.Core.Database.Entities;
using Moonlight.Features.ServiceManagement.Entities;
using Moonlight.Features.Ticketing.Entities.Enums;

namespace Moonlight.Features.Ticketing.Entities;

public class Ticket
{
    public int Id { get; set; }
    public User Creator { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Tries { get; set; } = "";
    public TicketPriority Priority { get; set; } = TicketPriority.Low;
    public bool Open { get; set; } = true;
    public Service? Service { get; set; }

    public List<TicketMessage> Messages { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}