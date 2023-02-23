using Moonlight.App.Database.Entities;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.Support;

public class SupportClientService : IDisposable
{
    private readonly SupportServerService SupportServerService;
    private readonly IdentityService IdentityService;
    private readonly MessageService MessageService;

    public EventHandler<SupportMessage> OnNewMessage;
    
    public EventHandler OnUpdateTyping;
    private List<string> TypingUsers = new();

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
        
        MessageService.Subscribe<SupportClientService, User>(
            $"support.{Self.Id}.admintyping", 
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
            await MessageService.Emit($"support.{Self.Id}.typing", Self);
        });

        return Task.CompletedTask;
    }

    #endregion
    
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
        MessageService.Unsubscribe($"support.{Self.Id}.admintyping", this);
    }
}