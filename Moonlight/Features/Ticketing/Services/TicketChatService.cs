using Microsoft.EntityFrameworkCore;
using Moonlight.Core.Event;
using Moonlight.Core.Event.Args;
using Moonlight.Core.Extensions;
using Moonlight.Core.Repositories;
using Moonlight.Core.Services;
using Moonlight.Features.Ticketing.Entities;
using Moonlight.Features.Ticketing.Entities.Enums;

namespace Moonlight.Features.Ticketing.Services;

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

    public async Task Update(bool open, TicketPriority priority) // Updated and syncs ticket states to all listeners
    {
        if (Ticket.Open != open)
        {
            Ticket.Open = open;
            
            if(open)
                await SendSystemMessage("Ticket has been opened");
            else
                await SendSystemMessage("Ticket has been closed");
        }

        if (Ticket.Priority != priority)
        {
            Ticket.Priority = priority;
            
            await SendSystemMessage($"Ticket priority to {priority}");
        }
        
        TicketRepository.Update(Ticket);

        await Events.OnTicketUpdated.InvokeAsync(Ticket);
    }

    public Task Stop() // Clear cache and stop listeners
    {
        Events.OnTicketMessage -= OnTicketMessage;
        Events.OnTicketUpdated -= OnTicketUpdated;
        
        MessageCache.Clear();
        
        return Task.CompletedTask;
    }

    #region Sending

    public async Task SendSystemMessage(string content) // use this to send a message shown in a seperator
    {
        // Build the message model
        var message = new TicketMessage()
        {
            Content = content,
            Attachment = null,
            CreatedAt = DateTime.UtcNow,
            Sender = null,
            IsSupport = IsSupporter
        };

        await SyncMessage(message);
    }

    public async Task SendMessage(string content, Stream? attachmentStream = null, string? attachmentName = null) // Regular send method
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

        await SyncMessage(message);
    }

    private async Task SyncMessage(TicketMessage message) // Use this function to save and sync function to others
    {
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

    #endregion

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