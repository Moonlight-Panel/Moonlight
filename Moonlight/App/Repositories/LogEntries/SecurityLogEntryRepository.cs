using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities.LogsEntries;

namespace Moonlight.App.Repositories.LogEntries;

public class SecurityLogEntryRepository : IDisposable
{
    private readonly DataContext DataContext;

    public SecurityLogEntryRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public SecurityLogEntry Add(SecurityLogEntry securityLogEntry)
    {
        var x = DataContext.SecurityLog.Add(securityLogEntry);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public DbSet<SecurityLogEntry> Get()
    {
        return DataContext.SecurityLog;
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}