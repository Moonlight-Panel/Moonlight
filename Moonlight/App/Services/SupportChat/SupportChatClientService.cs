using Microsoft.AspNetCore.Components.Forms;
using Moonlight.App.Database.Entities;
using Moonlight.App.Events;
using Moonlight.App.Services.Files;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.SupportChat;

public class SupportChatClientService : IDisposable
{
    private readonly EventSystem Event;
    private readonly BucketService BucketService;
    private readonly IdentityService IdentityService;
    private readonly SupportChatServerService ServerService;

    public Func<SupportChatMessage, Task>? OnMessage { get; set; }
    public Func<string[], Task>? OnTypingChanged { get; set; }

    private User? User;
    private readonly List<User> TypingUsers = new();

    public SupportChatClientService(
        EventSystem eventSystem,
        SupportChatServerService serverService,
        IdentityService identityService,
        BucketService bucketService)
    {
        Event = eventSystem;
        ServerService = serverService;
        IdentityService = identityService;
        BucketService = bucketService;
    }

    public async Task Start()
    {
        User = IdentityService.User;

        if (User != null)
        {
            await Event.On<SupportChatMessage>($"supportChat.{User.Id}.message", this, async message =>
            {
                if (OnMessage != null)
                {
                    if(message.Sender != null && message.Sender.Id == User.Id)
                        return;
                    
                    await OnMessage.Invoke(message);
                }
            });

            await Event.On<User>($"supportChat.{User.Id}.typing", this, async user =>
            {
                await HandleTyping(user);
            });
        }
    }

    public async Task<SupportChatMessage[]> GetMessages()
    {
        if (User == null)
            return Array.Empty<SupportChatMessage>();
        
        return await ServerService.GetMessages(User);
    }

    public async Task<SupportChatMessage> SendMessage(string content, IBrowserFile? browserFile = null)
    {
        if (User != null)
        {
            string? attachment = null;

            if (browserFile != null)
            {
                attachment = await BucketService.StoreFile(
                    "supportChat", 
                    browserFile.OpenReadStream(1024 * 1024 * 5),
                    browserFile.Name);
            }

            return await ServerService.SendMessage(User, content, User, attachment);
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
        await Event.Emit($"supportChat.{User!.Id}.typing", User);
    }

    public async void Dispose()
    {
        if (User != null)
        {
            await Event.Off($"supportChat.{User.Id}.message", this);
            await Event.Off($"supportChat.{User.Id}.typing", this);
        }
    }
}