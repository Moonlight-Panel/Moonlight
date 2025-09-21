using Hangfire.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Database;

public class CoreDataContext : DbContext
{
    private readonly AppConfiguration Configuration;
    
    public DbSet<User> Users { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Theme> Themes { get; set; }

    public CoreDataContext(AppConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(optionsBuilder.IsConfigured)
            return;

        var database = Configuration.Database;
        
        var connectionString = $"Host={database.Host};" +
                               $"Port={database.Port};" +
                               $"Database={database.Database};" +
                               $"Username={database.Username};" +
                               $"Password={database.Password}";

        optionsBuilder.UseNpgsql(connectionString, builder =>
        {
            builder.MigrationsHistoryTable("MigrationsHistory", "core");
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Model.SetDefaultSchema("core");
        
        base.OnModelCreating(modelBuilder);
        modelBuilder.OnHangfireModelCreating();

        modelBuilder.Ignore<ApplicationTheme>();
        modelBuilder.Entity<Theme>()
            .OwnsOne(x => x.Content, builder =>
            {
                builder.ToJson();
            });
    }
}