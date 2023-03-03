using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories.Subscriptions;

public class SubscriptionLimitRepository : IDisposable
{
    private readonly DataContext DataContext;

    public SubscriptionLimitRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }
    
    public DbSet<SubscriptionLimit> Get()
    {
        return DataContext.SubscriptionLimits;
    }

    public SubscriptionLimit Add(SubscriptionLimit subscription)
    {
        var x = DataContext.SubscriptionLimits.Add(subscription);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(SubscriptionLimit subscription)
    {
        DataContext.SubscriptionLimits.Update(subscription);
        DataContext.SaveChanges();
    }
    
    public void Delete(SubscriptionLimit subscription)
    {
        DataContext.SubscriptionLimits.Remove(subscription);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}