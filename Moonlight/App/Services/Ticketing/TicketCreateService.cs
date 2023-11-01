using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Database.Entities.Tickets;
using Moonlight.App.Database.Enums;
using Moonlight.App.Event;
using Moonlight.App.Extensions;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Ticketing;

public class TicketCreateService
{
    private readonly Repository<Ticket> TicketRepository;
    private readonly IdentityService IdentityService;

    public TicketCreateService(Repository<Ticket> ticketRepository, IdentityService identityService)
    {
        TicketRepository = ticketRepository;
        IdentityService = identityService;
    }

    public async Task<Ticket> Perform(string name, string description, string tries, Service? service)
    {
        var ticket = new Ticket()
        {
            Creator = IdentityService.CurrentUser,
            Service = service,
            Description = description,
            Tries = tries,
            Open = true,
            CreatedAt = DateTime.UtcNow,
            Name = name,
            Priority = TicketPriority.Low
        };

        var finalTicket = TicketRepository.Add(ticket);

        await Events.OnTicketCreated.InvokeAsync(finalTicket);
        
        return finalTicket;
    }
}