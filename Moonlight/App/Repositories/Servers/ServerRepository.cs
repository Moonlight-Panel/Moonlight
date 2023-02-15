using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories.Servers;

public class ServerRepository : IDisposable
{
    private readonly DataContext DataContext;

    public ServerRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<Server> Get()
    {
        return DataContext.Servers;
    }

    public Server Add(Server server)
    {
        var x = DataContext.Servers.Add(server);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(Server server)
    {
        DataContext.Servers.Update(server);
        DataContext.SaveChanges();
    }

    public void Delete(Server server)
    {
        DataContext.Servers.Remove(server);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}