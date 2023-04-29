using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;

namespace Moonlight.App.Repositories;

public class Repository<TEntity> where TEntity : class
{
    private readonly DataContext DataContext;
    private readonly DbSet<TEntity> DbSet;

    public Repository(DataContext dbContext)
    {
        DataContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = DataContext.Set<TEntity>();
    }

    public DbSet<TEntity> Get()
    {
        return DbSet;
    }

    public TEntity Add(TEntity entity)
    {
        var x = DbSet.Add(entity);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
        DataContext.SaveChanges();
    }
    
    public void Delete(TEntity entity)
    {
        DbSet.Remove(entity);
        DataContext.SaveChanges();
    }
}