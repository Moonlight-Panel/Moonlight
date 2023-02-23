using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories.Domains;

public class DomainRepository : IDisposable
{
    private readonly DataContext DataContext;

    public DomainRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<Domain> Get()
    {
        return DataContext.Domains;
    }

    public Domain Add(Domain domain)
    {
        var x = DataContext.Domains.Add(domain);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(Domain domain)
    {
        DataContext.Domains.Update(domain);
        DataContext.SaveChanges();
    }

    public void Delete(Domain domain)
    {
        DataContext.Domains.Remove(domain);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}