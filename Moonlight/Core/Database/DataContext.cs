using Microsoft.EntityFrameworkCore;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database.Entities;
using Moonlight.Features.Servers.Entities;

namespace Moonlight.Core.Database;

public class DataContext : DbContext
{
    private readonly ConfigService<CoreConfiguration> ConfigService;
    
    // Core
    public DbSet<User> Users { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    
    // Servers
    public DbSet<Server> Servers { get; set; }
    public DbSet<ServerAllocation> ServerAllocations { get; set; }
    public DbSet<ServerDockerImage> ServerDockerImages { get; set; }
    public DbSet<ServerImage> ServerImages { get; set; }
    public DbSet<ServerImageVariable> ServerImageVariables { get; set; }
    public DbSet<ServerNode> ServerNodes { get; set; }
    public DbSet<ServerVariable> ServerVariables { get; set; }
    public DbSet<ServerNetwork> ServerNetworks { get; set; }
    public DbSet<ServerSchedule> ServerSchedules { get; set; }
    public DbSet<ServerScheduleItem> ServerScheduleItems { get; set; }
    
    public DataContext(ConfigService<CoreConfiguration> configService)
    {
        ConfigService = configService;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = ConfigService.Get().Database;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}