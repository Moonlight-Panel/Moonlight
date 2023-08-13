using Microsoft.AspNetCore.Components.Forms;
using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Misc;
using Moonlight.App.Services.Files;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.Tickets;

public class TicketClientService
{
    private readonly TicketServerService TicketServerService;
    private readonly IdentityService IdentityService;
    private readonly BucketService BucketService;
    
    public Ticket? Ticket { get; set; }

    public TicketClientService(
        TicketServerService ticketServerService,
        IdentityService identityService,
        BucketService bucketService)
    {
        TicketServerService = ticketServerService;
        IdentityService = identityService;
        BucketService = bucketService;
    }

    public async Task<Ticket> Create(string issueTopic, string issueDescription, string issueTries, TicketSubject subject, int subjectId)
    {
        return await TicketServerService.Create(
            IdentityService.User,
            issueTopic,
            issueDescription,
            issueTries,
            subject,
            subjectId
        );
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
            attachment
        );
    }

    public async Task<TicketMessage[]> GetMessages()
    {
        return await TicketServerService.GetMessages(Ticket!);
    }
}