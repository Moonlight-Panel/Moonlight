using Moonlight.App.Database.Entities;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Notifications;

public class NotificationServerService
{
    private UserRepository UserRepository;
    private NotificationRepository NotificationRepository;

    private readonly IServiceScopeFactory ServiceScopeFactory;
    private IServiceScope ServiceScope;
    
    public NotificationServerService(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
        Task.Run(Run);
    }
    
    private Task Run()
    {
        ServiceScope = ServiceScopeFactory.CreateScope();

        UserRepository = ServiceScope
            .ServiceProvider
            .GetRequiredService<UserRepository>();

        NotificationRepository = ServiceScope
            .ServiceProvider
            .GetRequiredService<NotificationRepository>();
        
        return Task.CompletedTask;
    }

    private List<NotificationClientService> connectedClients = new();

    public List<NotificationClientService> GetConnectedClients(User user)
    {
        return connectedClients.Where(x => x.User == user).ToList();
    }

    public void AddClient(NotificationClientService notificationClientService)
    {
        connectedClients.Add(notificationClientService);
    }

    public void RemoveClient(NotificationClientService notificationClientService)
    {
        connectedClients.Remove(notificationClientService);
    }
}