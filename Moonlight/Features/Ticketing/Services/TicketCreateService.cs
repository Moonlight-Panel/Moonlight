using Moonlight.Core.Event;
using Moonlight.Core.Extensions;
using Moonlight.Core.Repositories;
using Moonlight.Core.Services;
using Moonlight.Features.ServiceManagement.Entities;
using Moonlight.Features.Ticketing.Entities;
using Moonlight.Features.Ticketing.Entities.Enums;

namespace Moonlight.Features.Ticketing.Services;

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