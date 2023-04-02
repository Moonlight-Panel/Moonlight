using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Models.Misc;

namespace Moonlight.App.Repositories;

public class CleanupExceptionRepository : IDisposable
{
    private readonly DataContext DataContext;

    public CleanupExceptionRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }
    
    public DbSet<CleanupException> Get()
    {
        return DataContext.CleanupExceptions;
    }

    public CleanupException Add(CleanupException cleanupException)
    {
        var x = DataContext.CleanupExceptions.Add(cleanupException).Entity;
        DataContext.SaveChanges();
        return x;
    }

    public void Update(CleanupException cleanupException)
    {
        DataContext.CleanupExceptions.Update(cleanupException);
        DataContext.SaveChanges();
    }

    public void Delete(CleanupException cleanupException)
    {
        DataContext.CleanupExceptions.Remove(cleanupException);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}