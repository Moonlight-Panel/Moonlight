using Microsoft.AspNetCore.Components.Forms;
using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Misc;
using Moonlight.App.Services.Files;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.Tickets;

public class TicketAdminService
{
    private readonly TicketServerService TicketServerService;
    private readonly IdentityService IdentityService;
    private readonly BucketService BucketService;

    public Ticket? Ticket { get; set; }

    public TicketAdminService(
        TicketServerService ticketServerService,
        IdentityService identityService,
        BucketService bucketService)
    {
        TicketServerService = ticketServerService;
        IdentityService = identityService;
        BucketService = bucketService;
    }

    public async Task<TicketMessage> Send(string content, IBrowserFile? file = null)
    {
        string? attachment = null;

        if (file != null)
        {
            attachment = await BucketService.StoreFile(
                "tickets",
                file.OpenReadStream(1024 * 1024 * 5),
                file.Name);
        }

        return await TicketServerService.SendMessage(
            Ticket!,
            IdentityService.User,
            content,
            attachment,
            true
        );
    }

    public async Task UpdateStatus(TicketStatus status)
    {
        await TicketServerService.UpdateStatus(Ticket!, status);
    }

    public async Task UpdatePriority(TicketPriority priority)
    {
        await TicketServerService.UpdatePriority(Ticket!, priority);
    }

    public async Task<TicketMessage[]> GetMessages()
    {
        return await TicketServerService.GetMessages(Ticket!);
    }

    public async Task SetClaim(User? user)
    {
        await TicketServerService.SetClaim(Ticket!, user);
    }
}