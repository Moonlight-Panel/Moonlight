using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories;

public class AaPanelRepository : IDisposable
{
    private readonly DataContext DataContext;

    public AaPanelRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<AaPanel> Get()
    {
        return DataContext.AaPanels;
    }

    public AaPanel Add(AaPanel aaPanel)
    {
        var x = DataContext.AaPanels.Add(aaPanel);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(AaPanel aaPanel)
    {
        DataContext.AaPanels.Update(aaPanel);
        DataContext.SaveChanges();
    }

    public void Delete(AaPanel aaPanel)
    {
        DataContext.AaPanels.Remove(aaPanel);
        DataContext.SaveChanges();
    }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}