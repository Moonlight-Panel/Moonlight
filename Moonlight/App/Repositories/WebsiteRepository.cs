using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories;

public class WebsiteRepository : IDisposable
{
    private readonly DataContext DataContext;

    public WebsiteRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<Website> Get()
    {
        return DataContext.Websites;
    }

    public Website Add(Website website)
    {
        var x = DataContext.Websites.Add(website);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(Website website)
    {
        DataContext.Websites.Update(website);
        DataContext.SaveChanges();
    }
    
    public void Delete(Website website)
    {
        DataContext.Websites.Remove(website);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}