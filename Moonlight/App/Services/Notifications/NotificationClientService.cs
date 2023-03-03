using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Notification;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.Notifications;

public class NotificationClientService
{
    private readonly NotificationRepository NotificationRepository;
    private readonly NotificationServerService NotificationServerService;
    
    public NotificationClientService(NotificationRepository notificationRepository, NotificationServerService notificationServerService)
    {
        NotificationRepository = notificationRepository;
        NotificationServerService = notificationServerService;
    }

    public User User => NotificationClient.User;
    
    public NotificationClient NotificationClient { get; set; }
}