using Moonlight.App.Database.Entities;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.Support;

public class SupportClientService : IDisposable
{
    private readonly SupportServerService SupportServerService;
    private readonly IdentityService IdentityService;
    private readonly MessageService MessageService;

    public EventHandler<SupportMessage> OnNewMessage;

    private User Self;
    
    public SupportClientService(
        SupportServerService supportServerService,
        IdentityService identityService,
        MessageService messageService)
    {
        SupportServerService = supportServerService;
        IdentityService = identityService;
        MessageService = messageService;
    }

    public async Task Start()
    {
        Self = (await IdentityService.Get())!;
        
        MessageService.Subscribe<SupportClientService, SupportMessage>(
            $"support.{Self.Id}.message", 
            this,  
            message =>
        {
            OnNewMessage?.Invoke(this, message);
            
            return Task.CompletedTask;
        });
    }

    public async Task<SupportMessage[]> GetMessages()
    {
        return await SupportServerService.GetMessages(Self);
    }

    public async Task SendMessage(string content)
    {
        var message = new SupportMessage()
        {
            Message = content
        };

        await SupportServerService.SendMessage(
            Self,
            message,
            Self
        );
    }

    public void Dispose()
    {
        MessageService.Unsubscribe($"support.{Self.Id}.message", this);
    }
}