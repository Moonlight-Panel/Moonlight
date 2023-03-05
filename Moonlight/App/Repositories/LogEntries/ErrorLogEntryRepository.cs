using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities.LogsEntries;

namespace Moonlight.App.Repositories.LogEntries;

public class ErrorLogEntryRepository : IDisposable
{
    private readonly DataContext DataContext;

    public ErrorLogEntryRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public ErrorLogEntry Add(ErrorLogEntry errorLogEntry)
    {
        var x = DataContext.ErrorLog.Add(errorLogEntry);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public DbSet<ErrorLogEntry> Get()
    {
        return DataContext.ErrorLog;
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}