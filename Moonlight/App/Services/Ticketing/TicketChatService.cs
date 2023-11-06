using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities.Tickets;
using Moonlight.App.Event;
using Moonlight.App.Event.Args;
using Moonlight.App.Extensions;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Ticketing;

public class TicketChatService
{
    private readonly IdentityService IdentityService;
    private readonly Repository<Ticket> TicketRepository;
    private readonly BucketService BucketService;
    private readonly List<TicketMessage> MessageCache = new();

    public Ticket Ticket;
    public bool IsSupporter;
    public Func<Task>? OnUpdate;
    public TicketMessage[] Messages => MessageCache.ToArray();

    public TicketChatService(
        IdentityService identityService,
        Repository<Ticket> ticketRepository,
        BucketService bucketService)
    {
        IdentityService = identityService;
        TicketRepository = ticketRepository;
        BucketService = bucketService;
    }

    public Task Start(Ticket ticket, bool isSupporter = false)
    {
        IsSupporter = isSupporter;
        
        // Load data into local cache
        Ticket = TicketRepository
            .Get()
            .Include(x => x.Messages)
            .Include(x => x.Creator)
            .Include(x => x.Service)
            .First(x => x.Id == ticket.Id);
        
        MessageCache.AddRange(Ticket.Messages);
        
        // Register event handlers
        Events.OnTicketMessage += OnTicketMessage;
        Events.OnTicketUpdated += OnTicketUpdated;
        
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        Events.OnTicketMessage -= OnTicketMessage;
        Events.OnTicketUpdated -= OnTicketUpdated;
        
        MessageCache.Clear();
        
        return Task.CompletedTask;
    }

    public async Task SendMessage(string content, Stream? attachmentStream = null, string? attachmentName = null)
    {
        if(string.IsNullOrEmpty(content))
            return;

        string? attachmentBucketName = null;

        // Check and download attachments
        if (attachmentStream != null && attachmentName != null)
        {
            attachmentBucketName = await BucketService.Store(
                "ticketAttachments",
                attachmentStream,
                attachmentName
            );
        }
        
        // Build the message model
        var message = new TicketMessage()
        {
            Content = content,
            Attachment = attachmentBucketName,
            CreatedAt = DateTime.UtcNow,
            Sender = IdentityService.CurrentUser,
            IsSupport = IsSupporter
        };

        // Save ticket to the db
        var t = TicketRepository
            .Get()
            .First(x => x.Id == Ticket.Id); // We do this to get a clean reference
        
        t.Messages.Add(message);
        TicketRepository.Update(t);
        
        // Now emit the events
        await Events.OnTicketMessage.InvokeAsync(new()
        {
            Ticket = t, // We use this reference as it has less data attached to it
            TicketMessage = message
        });
    }

    // Event handlers
    private async void OnTicketUpdated(object? _, Ticket ticket)
    {
        if(Ticket.Id != ticket.Id) // Only listen to our ticket
            return;

        // Update the possible values
        Ticket.Open = ticket.Open;
        Ticket.Priority = ticket.Priority;

        if (OnUpdate != null)
            await OnUpdate.Invoke();
    }
    
    private async void OnTicketMessage(object? _, TicketMessageEventArgs eventArgs)
    {
        if(Ticket.Id != eventArgs.Ticket.Id) // Only listen to our ticket
            return;
        
        MessageCache.Add(eventArgs.TicketMessage);

        if (OnUpdate != null)
            await OnUpdate.Invoke();
    }
}