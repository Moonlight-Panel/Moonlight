using System.Net.WebSockets;
using System.Text;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Notification;
using Moonlight.App.Http.Controllers.Api.Moonlight.Notifications;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.Notifications;

public class NotificationClientService
{
    private readonly NotificationRepository NotificationRepository;
    private readonly NotificationServerService NotificationServerService;
    internal ListenController listenController;
    
    public NotificationClientService(NotificationRepository notificationRepository, NotificationServerService notificationServerService)
    {
        NotificationRepository = notificationRepository;
        NotificationServerService = notificationServerService;
    }

    public User User => NotificationClient.User;
    
    public NotificationClient NotificationClient { get; set; }

    public async Task SendAction(string action)
    {
        await listenController.ws.SendAsync(Encoding.UTF8.GetBytes(action), WebSocketMessageType.Text,
            WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
    }

    public void WebsocketReady(NotificationClient client)
    {
        NotificationClient = client;
        NotificationServerService.AddClient(this);
    }

    public void WebsocketClosed()
    {
        NotificationServerService.RemoveClient(this);
    }
}