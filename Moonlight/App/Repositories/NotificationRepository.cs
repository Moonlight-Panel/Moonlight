using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Notification;

namespace Moonlight.App.Repositories;

public class NotificationRepository : IDisposable
{
    private readonly DataContext DataContext;

    public NotificationRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public int RegisterNewDevice(User user)
    {
        var x = DataContext.NotificationClients.Add(new NotificationClient() {User = user});
        DataContext.SaveChanges();
        return x.Entity.Id;
    }

    public DbSet<NotificationClient> GetClients() => DataContext.NotificationClients;
    public DbSet<NotificationAction> GetActions() => DataContext.NotificationActions;

    public void Dispose()
    {
        DataContext.Dispose();
    }

    public void AddAction(NotificationAction action)
    {
        DataContext.NotificationActions.Add(action);
        DataContext.SaveChanges();
    }

    public void RemoveAction(NotificationAction action)
    {
        DataContext.NotificationActions.Remove(action);
        DataContext.SaveChanges();
    }
}