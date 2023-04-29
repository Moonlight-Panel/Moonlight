using Logging.Net;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Support;

public class SupportServerService : IDisposable
{
    private SupportMessageRepository SupportMessageRepository;
    private MessageService MessageService;
    private UserRepository UserRepository;
    private readonly IServiceScopeFactory ServiceScopeFactory;
    private IServiceScope ServiceScope;
    
    public SupportServerService(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
        
        Task.Run(Run);
    }

    public async Task SendMessage(User r, SupportMessage message, User s, bool isSupport = false)
    {
        var recipient = UserRepository.Get().First(x => x.Id == r.Id);
        var sender = UserRepository.Get().First(x => x.Id == s.Id);

        Task.Run(async () =>
        {
            try
            {
                message.CreatedAt = DateTime.UtcNow;
                message.Sender = sender;
                message.Recipient = recipient;
                message.IsSupport = isSupport;

                SupportMessageRepository.Add(message);

                await MessageService.Emit($"support.{recipient.Id}.message", message);

                if (!recipient.SupportPending)
                {
                    recipient.SupportPending = true;
                    UserRepository.Update(recipient);

                    if (!message.IsSupport)
                    {
                        var systemMessage = new SupportMessage()
                        {
                            Recipient = recipient,
                            Sender = null,
                            IsSystem = true,
                            Message = "The support team has been notified. Please be patient"
                        };

                        SupportMessageRepository.Add(systemMessage);

                        await MessageService.Emit($"support.{recipient.Id}.message", systemMessage);
                    }
                
                    await MessageService.Emit($"support.new", recipient);

                    Logger.Info("Support ticket created: " + recipient.Id);
                    //TODO: Ping or so
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error sending message");
                Logger.Error(e);
            }
        });
    }

    public async Task Close(User user)
    {
        var recipient = UserRepository.Get().First(x => x.Id == user.Id);
        
        recipient.SupportPending = false;
        UserRepository.Update(recipient);

        var systemMessage = new SupportMessage()
        {
            Recipient = recipient,
            Sender = null,
            IsSystem = true,
            Message = "The ticket is now closed. Type a message to open it again"
        };

        SupportMessageRepository.Add(systemMessage);

        await MessageService.Emit($"support.{recipient.Id}.message", systemMessage);
        await MessageService.Emit($"support.close", recipient);
    }

    public Task<SupportMessage[]> GetMessages(User r)
    {
        var recipient = UserRepository.Get().First(x => x.Id == r.Id);
        
        var messages = SupportMessageRepository
            .Get()
            .Include(x => x.Recipient)
            .Include(x => x.Sender)
            .Where(x => x.Recipient.Id == recipient.Id)
            .AsEnumerable()
            .TakeLast(50)
            .OrderBy(x => x.Id)
            .ToArray();

        return Task.FromResult(messages);
    }
    
    private Task Run()
    {
        ServiceScope = ServiceScopeFactory.CreateScope();

        SupportMessageRepository = ServiceScope
            .ServiceProvider
            .GetRequiredService<SupportMessageRepository>();

        MessageService = ServiceScope
            .ServiceProvider
            .GetRequiredService<MessageService>();

        UserRepository = ServiceScope
            .ServiceProvider
            .GetRequiredService<UserRepository>();
        
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        SupportMessageRepository.Dispose();
        UserRepository.Dispose();
        ServiceScope.Dispose();
    }
}