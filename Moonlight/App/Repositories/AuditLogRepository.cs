using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories;

public class AuditLogRepository : IDisposable
{
    private readonly DataContext DataContext;

    public AuditLogEntry Add(AuditLogEntry entry)
    {
        var x = DataContext.AuditLog.Add(entry);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}