using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories;

public class DdosAttackRepository
{
    private readonly DataContext DataContext;

    public DdosAttackRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<DdosAttack> Get()
    {
        return DataContext.DdosAttacks;
    }

    public DdosAttack Add(DdosAttack ddosAttack)
    {
        var x = DataContext.DdosAttacks.Add(ddosAttack);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(DdosAttack ddosAttack)
    {
        DataContext.DdosAttacks.Update(ddosAttack);
        DataContext.SaveChanges();
    }

    public void Delete(DdosAttack ddosAttack)
    {
        DataContext.DdosAttacks.Remove(ddosAttack);
        DataContext.SaveChanges();
    }
}