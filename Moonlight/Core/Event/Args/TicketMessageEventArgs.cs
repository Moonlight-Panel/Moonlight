using Moonlight.Features.Ticketing.Entities;

namespace Moonlight.Core.Event.Args;

public class TicketMessageEventArgs
{
    public Ticket Ticket { get; set; }
    public TicketMessage TicketMessage { get; set; }
}