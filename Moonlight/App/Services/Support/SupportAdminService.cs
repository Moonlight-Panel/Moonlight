using Moonlight.App.Database.Entities;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.Support;

public class SupportAdminService
{
    private readonly SupportServerService SupportServerService;
    private readonly IdentityService IdentityService;
    private readonly MessageService MessageService;

    public EventHandler<SupportMessage> OnNewMessage;

    public EventHandler OnUpdateTyping;
    private List<string> TypingUsers = new();
    
    private User Self;
    private User Recipient;

    public SupportAdminService(
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
        
        MessageService.Subscribe<SupportClientService, User>(
            $"support.{Self.Id}.typing",
            this,  
            user =>
            {
                HandleTyping(user);
                return Task.CompletedTask;
            });
    }
    
    #region Typing
    
    private void HandleTyping(User user)
    {
        var name = $"{user.FirstName} {user.LastName}";
        
        lock (TypingUsers)
        {
            if (!TypingUsers.Contains(name))
            {
                TypingUsers.Add(name);
                OnUpdateTyping!.Invoke(this, null!);

                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    if (TypingUsers.Contains(name))
                    {
                        TypingUsers.Remove(name);
                        OnUpdateTyping!.Invoke(this, null!);
                    }
                });
            }
        }
    }
    
    public string[] GetTypingUsers()
    {
        lock (TypingUsers)
        {
            return TypingUsers.ToArray();
        }
    }
    
    public Task TriggerTyping()
    {
        Task.Run(async () =>
        {
            await MessageService.Emit($"support.{Recipient.Id}.admintyping", Self);
        });

        return Task.CompletedTask;
    }
    
    #endregion

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

    public async Task Close()
    {
        await SupportServerService.Close(Recipient);
    }

    public void Dispose()
    {
        MessageService.Unsubscribe($"support.{Recipient.Id}.message", this);
        MessageService.Unsubscribe($"support.{Recipient.Id}.typing", this);
    }
}