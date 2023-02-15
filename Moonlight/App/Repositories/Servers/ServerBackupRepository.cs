using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories.Servers;

public class ServerBackupRepository : IDisposable
{
    private readonly DataContext DataContext;

    public ServerBackupRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<ServerBackup> Get()
    {
        return DataContext.ServerBackups;
    }

    public ServerBackup Add(ServerBackup serverBackup)
    {
        var x = DataContext.ServerBackups.Add(serverBackup);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(ServerBackup serverBackup)
    {
        DataContext.ServerBackups.Update(serverBackup);
        DataContext.SaveChanges();
    }

    public void Delete(ServerBackup serverBackup)
    {
        DataContext.ServerBackups.Remove(serverBackup);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}