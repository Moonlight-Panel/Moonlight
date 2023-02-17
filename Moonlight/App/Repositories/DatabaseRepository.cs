using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;

namespace Moonlight.App.Repositories;

public class DatabaseRepository : IDisposable
{
    private readonly DataContext DataContext;

    public DatabaseRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<Database.Entities.Database> Get()
    {
        return DataContext.Databases;
    }

    public Database.Entities.Database Add(Database.Entities.Database database)
    {
        var x = DataContext.Databases.Add(database);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(Database.Entities.Database database)
    {
        DataContext.Databases.Update(database);
        DataContext.SaveChanges();
    }

    public void Delete(Database.Entities.Database database)
    {
        DataContext.Databases.Remove(database);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}