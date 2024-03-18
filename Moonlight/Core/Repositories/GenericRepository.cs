using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using Moonlight.Core.Database;

namespace Moonlight.Core.Repositories;

public class GenericRepository<TEntity> : Repository<TEntity> where TEntity : class
{
    private readonly DataContext DataContext;
    private readonly DbSet<TEntity> DbSet;

    public GenericRepository(DataContext dbContext)
    {
        DataContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = DataContext.Set<TEntity>();
    }

    public override DbSet<TEntity> Get()
    {
        return DbSet;
    }

    public override TEntity Add(TEntity entity)
    {
        var x = DbSet.Add(entity);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public override void Update(TEntity entity)
    {
        DbSet.Update(entity);
        DataContext.SaveChanges();
    }
    
    public override void Delete(TEntity entity)
    {
        DbSet.Remove(entity);
        DataContext.SaveChanges();
    }
}