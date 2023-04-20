using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Events;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.SupportChat;

public class SupportChatServerService
{
    private readonly IServiceScopeFactory ServiceScopeFactory;
    private readonly DateTimeService DateTimeService;
    private readonly EventSystem Event;

    public SupportChatServerService(
        IServiceScopeFactory serviceScopeFactory,
        DateTimeService dateTimeService,
        EventSystem eventSystem)
    {
        ServiceScopeFactory = serviceScopeFactory;
        DateTimeService = dateTimeService;
        Event = eventSystem;
    }

    public Task<SupportChatMessage[]> GetMessages(User recipient)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var msgRepo = scope.ServiceProvider.GetRequiredService<Repository<SupportChatMessage>>();

        var messages = msgRepo
            .Get()
            .Include(x => x.Recipient)
            .Include(x => x.Sender)
            .Where(x => x.Recipient.Id == recipient.Id)
            .OrderByDescending(x => x.CreatedAt)
            .AsEnumerable()
            .Take(50)
            .ToArray();

        return Task.FromResult(messages);
    }

    public async Task SendMessage(User recipient, string content, User? sender, string? attachment = null)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var msgRepo = scope.ServiceProvider.GetRequiredService<Repository<SupportChatMessage>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();

        var message = new SupportChatMessage()
        {
            CreatedAt = DateTimeService.GetCurrent(),
            IsQuestion = false,
            Sender = sender == null ? null : userRepo.Get().First(x => x.Id == sender.Id),
            Recipient = userRepo.Get().First(x => x.Id == recipient.Id),
            Answer = "",
            Attachment = attachment ?? "",
            Content = content,
            UpdatedAt = DateTimeService.GetCurrent()
        };

        var finalMessage = msgRepo.Add(message);

        await Event.Emit($"supportChat.{recipient.Id}.message", finalMessage);
        await Event.Emit("supportChat.message", finalMessage);

        if (!userRepo.Get().First(x => x.Id == recipient.Id).SupportPending)
        {
            var ticketStart = new SupportChatMessage()
            {
                CreatedAt = DateTimeService.GetCurrent(),
                IsQuestion = false,
                Sender = null,
                Recipient = userRepo.Get().First(x => x.Id == recipient.Id),
                Answer = "",
                Attachment = "",
                Content = "Support ticket open", //TODO: Config
                UpdatedAt = DateTimeService.GetCurrent()
            };

            var ticketStartFinal = msgRepo.Add(ticketStart);

            var user = userRepo.Get().First(x => x.Id == recipient.Id);
            user.SupportPending = true;
            userRepo.Update(user);
            
            await Event.Emit($"supportChat.{recipient.Id}.message", ticketStartFinal);
            await Event.Emit("supportChat.message", ticketStartFinal);
            await Event.Emit("supportChat.new", recipient);
        }
    }

    public Task<Dictionary<User, SupportChatMessage?>> GetOpenChats()
    {
        var result = new Dictionary<User, SupportChatMessage?>();

        using var scope = ServiceScopeFactory.CreateScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
        var msgRepo = scope.ServiceProvider.GetRequiredService<Repository<SupportChatMessage>>();

        foreach (var user in userRepo.Get().Where(x => x.SupportPending).ToArray())
        {
            var lastMessage = msgRepo
                .Get()
                .Include(x => x.Recipient)
                .Include(x => x.Sender)
                .Where(x => x.Recipient.Id == user.Id)
                .OrderByDescending(x => x.CreatedAt)
                .AsEnumerable()
                .FirstOrDefault();
            
            result.Add(user, lastMessage);
        }

        return Task.FromResult(result);
    }

    public async Task CloseChat(User recipient)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var msgRepo = scope.ServiceProvider.GetRequiredService<Repository<SupportChatMessage>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
        
        var ticketEnd = new SupportChatMessage()
        {
            CreatedAt = DateTimeService.GetCurrent(),
            IsQuestion = false,
            Sender = null,
            Recipient = userRepo.Get().First(x => x.Id == recipient.Id),
            Answer = "",
            Attachment = "",
            Content = "Support ticket closed", //TODO: Config
            UpdatedAt = DateTimeService.GetCurrent()
        };

        var ticketEndFinal = msgRepo.Add(ticketEnd);

        var user = userRepo.Get().First(x => x.Id == recipient.Id);
        user.SupportPending = false;
        userRepo.Update(user);
            
        await Event.Emit($"supportChat.{recipient.Id}.message", ticketEndFinal);
        await Event.Emit("supportChat.message", ticketEndFinal);
        await Event.Emit("supportChat.close", recipient);
    }
}