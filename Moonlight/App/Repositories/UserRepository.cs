using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Repositories;

public class UserRepository : IDisposable
{
    private readonly DataContext DataContext;

    public UserRepository(DataContext dataContext)
    {
        DataContext = dataContext;
    }

    public DbSet<User> Get()
    {
        return DataContext.Users;
    }

    public User Add(User user)
    {
        var x = DataContext.Users.Add(user);
        DataContext.SaveChanges();
        return x.Entity;
    }

    public void Update(User user)
    {
        DataContext.Users.Update(user);
        DataContext.SaveChanges();
    }

    public void Delete(User user)
    {
        DataContext.Users.Remove(user);
        DataContext.SaveChanges();
    }
    
    public void Dispose()
    {
        DataContext.Dispose();
    }
}