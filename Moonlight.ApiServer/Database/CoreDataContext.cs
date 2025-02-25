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
    
    public CoreDataContext(DatabaseOptions options) : base(options)
    {
    }
}