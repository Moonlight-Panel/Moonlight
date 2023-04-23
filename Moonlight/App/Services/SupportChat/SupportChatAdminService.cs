using Moonlight.App.Database.Entities;
using Moonlight.App.Events;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.SupportChat;

public class SupportChatAdminService
{
    private readonly EventSystem Event;
    private readonly IdentityService IdentityService;
    private readonly SupportChatServerService ServerService;
    
    public Func<SupportChatMessage, Task>? OnMessage { get; set; }
    public Func<string[], Task>? OnTypingChanged { get; set; }

    private User? User;
    private User Recipient = null!;
    private readonly List<User> TypingUsers = new();

    public SupportChatAdminService(
        EventSystem eventSystem,
        SupportChatServerService serverService,
        IdentityService identityService)
    {
        Event = eventSystem;
        ServerService = serverService;
        IdentityService = identityService;
    }

    public async Task Start(User recipient)
    {
        User = await IdentityService.Get();
        Recipient = recipient;

        if (User != null)
        {
            await Event.On<SupportChatMessage>($"supportChat.{Recipient.Id}.message", this, async message =>
            {
                if(OnMessage != null)
                    await OnMessage.Invoke(message);
            });

            await Event.On<User>($"supportChat.{Recipient.Id}.typing", this, async user =>
            {
                await HandleTyping(user);
            });
        }
    }

    public async Task<SupportChatMessage[]> GetMessages()
    {
        if (User == null)
            return Array.Empty<SupportChatMessage>();
        
        return await ServerService.GetMessages(Recipient);
    }

    public async Task<SupportChatMessage> SendMessage(string content)
    {
        if (User != null)
        {
            return await ServerService.SendMessage(Recipient, content, User);
        }

        return null!;
    }
    
    private Task HandleTyping(User user)
    {
        lock (TypingUsers)
        {
            if (!TypingUsers.Contains(user))
            {
                TypingUsers.Add(user);

                if (OnTypingChanged != null)
                {
                    OnTypingChanged.Invoke(
                        TypingUsers
                            .Where(x => x.Id != User!.Id)
                            .Select(x => $"{x.FirstName} {x.LastName}")
                            .ToArray()
                    );
                }

                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    if (TypingUsers.Contains(user))
                    {
                        TypingUsers.Remove(user);

                        if (OnTypingChanged != null)
                        {
                            await OnTypingChanged.Invoke(
                                TypingUsers
                                    .Where(x => x.Id != User!.Id)
                                    .Select(x => $"{x.FirstName} {x.LastName}")
                                    .ToArray()
                            );
                        }
                    }
                });
            }
        }
        
        return Task.CompletedTask;
    }
    
    public async Task SendTyping()
    {
        await Event.Emit($"supportChat.{Recipient.Id}.typing", User);
    }

    public async Task Close()
    {
        await ServerService.CloseChat(Recipient);
    }

    public async void Dispose()
    {
        if (User != null)
        {
            await Event.Off($"supportChat.{Recipient.Id}.message", this);
            await Event.Off($"supportChat.{Recipient.Id}.typing", this);
        }
    }
}