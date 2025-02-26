using Microsoft.EntityFrameworkCore;
using MoonCore.Extended.SingleDb;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers;

namespace Moonlight.ApiServer.Database;

public class CoreDataContext : DatabaseContext
{
    public override string Prefix { get; } = "Core";
    
    public DbSet<User> Users { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    
    public CoreDataContext(AppConfiguration configuration)
    {
        Options = new()
        {
            Host = configuration.Database.Host,
            Port = configuration.Database.Port,
            Username = configuration.Database.Username,
            Password = configuration.Database.Password,
            Database = configuration.Database.Database
        };
    }
}