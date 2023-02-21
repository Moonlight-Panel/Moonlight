using Moonlight.App.Database.Entities;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.Support;

public class SupportAdminServer
{
    private readonly SupportServerService SupportServerService;
    private readonly IdentityService IdentityService;
    private readonly MessageService MessageService;

    public EventHandler<SupportMessage> OnNewMessage;

    private User Self;
    private User Recipient;

    public SupportAdminServer(
        SupportServerService supportServerService,
        IdentityService identityService,
        MessageService messageService)
    {
        SupportServerService = supportServerService;
        IdentityService = identityService;
        MessageService = messageService;
    }

    public async Task Start(User user)
    {
        Self = (await IdentityService.Get())!;
        Recipient = user;

        MessageService.Subscribe<SupportClientService, SupportMessage>(
            $"support.{Recipient.Id}.message",
            this,
            message =>
            {
                OnNewMessage?.Invoke(this, message);

                return Task.CompletedTask;
            });
    }

    public async Task<SupportMessage[]> GetMessages()
    {
        return await SupportServerService.GetMessages(Recipient);
    }

    public async Task SendMessage(string content)
    {
        var message = new SupportMessage()
        {
            Message = content
        };

        await SupportServerService.SendMessage(
            Recipient,
            message,
            Self,
            true
        );
    }

    public void Dispose()
    {
        MessageService.Unsubscribe($"support.{Recipient.Id}.message", this);
    }
}