using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities.LogsEntries;

namespace Moonlight.App.Repositories.LogEntries;

public class AuditLogEntryRepository : IDisposable
{
    private readonly DataContext DataContext;

    public AuditLogEntryRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public AuditLogEntry Add(AuditLogEntry entry)
    {
        var x = DataContext.AuditLog.Add(entry);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public DbSet<AuditLogEntry> Get()
    {
        return DataContext.AuditLog;
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}