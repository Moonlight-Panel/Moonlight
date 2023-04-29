using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories;

public class SupportMessageRepository : IDisposable
{
    private readonly DataContext DataContext;

    public SupportMessageRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<SupportMessage> Get()
    {
        return DataContext.SupportMessages;
    }

    public SupportMessage Add(SupportMessage message)
    {
        var x = DataContext.SupportMessages.Add(message);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(SupportMessage message)
    {
        DataContext.SupportMessages.Update(message);
        DataContext.SaveChanges();
    }

    public void Delete(SupportMessage message)
    {
        DataContext.SupportMessages.Remove(message);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}