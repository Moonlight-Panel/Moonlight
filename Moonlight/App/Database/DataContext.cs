using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Services;

namespace Moonlight.App.Database;

public class DataContext : DbContext
{
    private readonly ConfigService ConfigService;
    
    public DbSet<User> Users { get; set; }

    public DataContext(ConfigService configService)
    {
        ConfigService = configService;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = ConfigService.Get().Database;

            if (config.UseSqlite)
                optionsBuilder.UseSqlite($"Data Source={config.SqlitePath}");
            else
            {
                var connectionString = $"host={config.Host};" +
                                       $"port={config.Port};" +
                                       $"database={config.Database};" +
                                       $"uid={config.Username};" +
                                       $"pwd={config.Password}";

                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    builder => builder.EnableRetryOnFailure(5)
                );
            }
        }
    }
}