using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories;

public class NewsEntryRepository
{
    private readonly DataContext DataContext;

    public NewsEntryRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<NewsEntry> Get()
    {
        return DataContext.NewsEntries;
    }

    public NewsEntry Add(NewsEntry newsEntry)
    {
        var x = DataContext.NewsEntries.Add(newsEntry);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(NewsEntry newsEntry)
    {
        DataContext.NewsEntries.Update(newsEntry);
        DataContext.SaveChanges();
    }

    public void Delete(NewsEntry newsEntry)
    {
        DataContext.NewsEntries.Remove(newsEntry);
        DataContext.SaveChanges();
    }
    
    public void Dispose()
    {
        DataContext.Dispose();
    }
}