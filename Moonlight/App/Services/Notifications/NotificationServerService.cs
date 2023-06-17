using System.Net.WebSockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Notification;
using Moonlight.App.Events;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Notifications;

public class NotificationServerService
{
    private readonly List<ActiveNotificationClient> ActiveClients = new();

    private readonly IServiceScopeFactory ServiceScopeFactory;
    private readonly EventSystem Event;

    public NotificationServerService(IServiceScopeFactory serviceScopeFactory, EventSystem eventSystem)
    {
        ServiceScopeFactory = serviceScopeFactory;
        Event = eventSystem;
    }

    public Task<ActiveNotificationClient[]> GetActiveClients()
    {
        lock (ActiveClients)
        {
            return Task.FromResult(ActiveClients.ToArray());
        }
    }

    public Task<ActiveNotificationClient[]> GetUserClients(User user)
    {
        lock (ActiveClients)
        {
            return Task.FromResult(
                ActiveClients
                    .Where(x => x.Client.User.Id == user.Id)
                    .ToArray()
            );
        }
    }

    public async Task SendAction(User user, string action)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var notificationClientRepository =
            scope.ServiceProvider.GetRequiredService<Repository<NotificationClient>>();
        
        var clients = notificationClientRepository
            .Get()
            .Include(x => x.User)
            .Where(x => x.User == user)
            .ToList();

        foreach (var client in clients)
        {
            ActiveNotificationClient[] connectedUserClients;

            lock (ActiveClients)
            {
                connectedUserClients = ActiveClients
                    .Where(x => x.Client.Id == user.Id)
                    .ToArray();
            }
            
            if (connectedUserClients.Length > 0)
            {
                await connectedUserClients[0].SendAction(action);
            }
            else
            {
                var notificationAction = new NotificationAction()
                {
                    Action = action,
                    NotificationClient = client
                };
                
                var notificationActionsRepository =
                    scope.ServiceProvider.GetRequiredService<Repository<NotificationAction>>();

                notificationActionsRepository.Add(notificationAction);
            }
        }
    }

    public async Task RegisterClient(WebSocket webSocket, NotificationClient notificationClient)
    {
        var newClient = new ActiveNotificationClient()
        {
            WebSocket = webSocket,
            Client = notificationClient
        };
        
        lock (ActiveClients)
        {
            ActiveClients.Add(newClient);
        }

        await Event.Emit("notifications.addClient", notificationClient);
    }

    public async Task UnRegisterClient(NotificationClient client)
    {
        lock (ActiveClients)
        {
            ActiveClients.RemoveAll(x => x.Client == client);
        }
        
        await Event.Emit("notifications.removeClient", client);
    }
}