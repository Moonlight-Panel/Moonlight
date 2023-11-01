using Moonlight.App.Database.Entities.Tickets;

namespace Moonlight.App.Event.Args;

public class TicketMessageEventArgs
{
    public Ticket Ticket { get; set; }
    public TicketMessage TicketMessage { get; set; }
}