using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories.Domains;

public class SharedDomainRepository : IDisposable
{
    private readonly DataContext DataContext;

    public SharedDomainRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<SharedDomain> Get()
    {
        return DataContext.SharedDomains;
    }

    public SharedDomain Add(SharedDomain sharedDomain)
    {
        var x = DataContext.SharedDomains.Add(sharedDomain);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(SharedDomain sharedDomain)
    {
        DataContext.SharedDomains.Update(sharedDomain);
        DataContext.SaveChanges();
    }

    public void Delete(SharedDomain domain)
    {
        DataContext.SharedDomains.Remove(domain);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}