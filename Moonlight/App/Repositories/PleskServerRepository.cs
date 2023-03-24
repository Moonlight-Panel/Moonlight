using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories;

public class PleskServerRepository : IDisposable
{
    private readonly DataContext DataContext;

    public PleskServerRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<PleskServer> Get()
    {
        return DataContext.PleskServers;
    }

    public PleskServer Add(PleskServer pleskServer)
    {
        var x = DataContext.PleskServers.Add(pleskServer);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(PleskServer pleskServer)
    {
        DataContext.PleskServers.Update(pleskServer);
        DataContext.SaveChanges();
    }
    
    public void Delete(PleskServer pleskServer)
    {
        DataContext.PleskServers.Remove(pleskServer);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}