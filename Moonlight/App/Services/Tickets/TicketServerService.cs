using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Events;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Tickets;

public class TicketServerService
{
    private readonly IServiceScopeFactory ServiceScopeFactory;
    private readonly EventSystem Event;
    private readonly ConfigService ConfigService;

    public TicketServerService(
        IServiceScopeFactory serviceScopeFactory,
        EventSystem eventSystem,
        ConfigService configService)
    {
        ServiceScopeFactory = serviceScopeFactory;
        Event = eventSystem;
        ConfigService = configService;
    }

    public async Task<Ticket> Create(User creator, string issueTopic, string issueDescription, string issueTries, TicketSubject subject, int subjectId)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<Repository<Ticket>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();

        var creatorUser = userRepo
            .Get()
            .First(x => x.Id == creator.Id);

        var ticket = ticketRepo.Add(new()
        {
            Priority = TicketPriority.Low,
            Status = TicketStatus.Open,
            AssignedTo = null,
            IssueTopic = issueTopic,
            IssueDescription = issueDescription,
            IssueTries = issueTries,
            Subject = subject,
            SubjectId = subjectId,
            CreatedBy = creatorUser
        });

        await Event.Emit("tickets.new", ticket);

        // Do automatic stuff here
        await SendSystemMessage(ticket, ConfigService.Get().Moonlight.Tickets.WelcomeMessage);

        if (ticket.Subject != TicketSubject.Other)
        {
            await SendMessage(ticket, creatorUser, $"Subject :\n\n{ticket.Subject}: {ticket.SubjectId}");
        }
        
        //TODO: Check for opening times
        
        return ticket;
    }
    public async Task SendSystemMessage(Ticket t, string content, string? attachmentUrl = null)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<Repository<Ticket>>();

        var ticket = ticketRepo.Get().First(x => x.Id == t.Id);

        var message = new TicketMessage()
        {
            Content = content,
            Sender = null,
            AttachmentUrl = attachmentUrl,
            IsSystemMessage = true
        };
        
        ticket.Messages.Add(message);
        ticketRepo.Update(ticket);

        await Event.Emit("tickets.message", message);
        await Event.Emit($"tickets.{ticket.Id}.message", message);
    }
    public async Task UpdatePriority(Ticket t, TicketPriority priority)
    {
        if(t.Priority == priority)
            return;
        
        using var scope = ServiceScopeFactory.CreateScope();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<Repository<Ticket>>();

        var ticket = ticketRepo.Get().First(x => x.Id == t.Id);

        ticket.Priority = priority;
        
        ticketRepo.Update(ticket);

        await Event.Emit("tickets.status", ticket);
        await Event.Emit($"tickets.{ticket.Id}.status", ticket);
        
        await SendSystemMessage(ticket, $"The ticket priority has been changed to: {priority}");
    }
    public async Task UpdateStatus(Ticket t, TicketStatus status)
    {
        if(t.Status == status)
            return;
        
        using var scope = ServiceScopeFactory.CreateScope();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<Repository<Ticket>>();

        var ticket = ticketRepo.Get().First(x => x.Id == t.Id);

        ticket.Status = status;
        
        ticketRepo.Update(ticket);
        
        await Event.Emit("tickets.status", ticket);
        await Event.Emit($"tickets.{ticket.Id}.status", ticket);

        await SendSystemMessage(ticket, $"The ticket status has been changed to: {status}");
    }
    public async Task<TicketMessage> SendMessage(Ticket t, User sender, string content, string? attachmentUrl = null, bool isSupportMessage = false)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<Repository<Ticket>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();

        var ticket = ticketRepo.Get().First(x => x.Id == t.Id);
        var user = userRepo.Get().First(x => x.Id == sender.Id);

        var message = new TicketMessage()
        {
            Content = content,
            Sender = user,
            AttachmentUrl = attachmentUrl,
            IsSupportMessage = isSupportMessage
        };
        
        ticket.Messages.Add(message);
        ticketRepo.Update(ticket);

        await Event.Emit("tickets.message", message);
        await Event.Emit($"tickets.{ticket.Id}.message", message);

        return message;
    }
    
    public Task<TicketMessage[]> GetMessages(Ticket ticket)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<Repository<Ticket>>();

        var tickets = ticketRepo
            .Get()
            .Include(x => x.CreatedBy)
            .Include(x => x.Messages)
            .First(x => x.Id == ticket.Id);

        return Task.FromResult(tickets.Messages.ToArray());
    }

    public async Task SetClaim(Ticket t, User? u = null)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<Repository<Ticket>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();

        var ticket = ticketRepo.Get().Include(x => x.AssignedTo).First(x => x.Id == t.Id);
        var user = u == null ? u : userRepo.Get().First(x => x.Id == u.Id);

        ticket.AssignedTo = user;
        
        ticketRepo.Update(ticket);
        
        await Event.Emit("tickets.status", ticket);
        await Event.Emit($"tickets.{ticket.Id}.status", ticket);

        var claimName = user == null ? "None" : user.FirstName + " " + user.LastName; 
        await SendSystemMessage(ticket, $"Ticked claim has been set to {claimName}");
    }
}