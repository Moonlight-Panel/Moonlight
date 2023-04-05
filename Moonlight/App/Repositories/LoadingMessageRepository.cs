using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories;

public class LoadingMessageRepository : IDisposable
{
    private readonly DataContext DataContext;

    public LoadingMessageRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<LoadingMessage> Get()
    {
        return DataContext.LoadingMessages;
    }

    public LoadingMessage Add(LoadingMessage loadingMessage)
    {
        var x = DataContext.LoadingMessages.Add(loadingMessage);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(LoadingMessage loadingMessage)
    {
        DataContext.LoadingMessages.Update(loadingMessage);
        DataContext.SaveChanges();
    }

    public void Delete(LoadingMessage loadingMessage)
    {
        DataContext.LoadingMessages.Remove(loadingMessage);
        DataContext.SaveChanges();
    }
    
    public void Dispose()
    {
        DataContext.Dispose();
    }
}