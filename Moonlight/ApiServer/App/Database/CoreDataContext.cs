using Microsoft.EntityFrameworkCore;
using MoonCore.Services;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Helpers.Database;
using Moonlight.Shared.Models;

namespace Moonlight.ApiServer.App.Database;

public class CoreDataContext : DatabaseContext
{
    public override string Prefix => "Core";

    public DbSet<User> Users { get; set; }

    public CoreDataContext()
    {
        SetupAsMigrationInstance();
    }
    
    public CoreDataContext(ConfigService<AppConfiguration> configService) : base(configService)
    {
    }
}