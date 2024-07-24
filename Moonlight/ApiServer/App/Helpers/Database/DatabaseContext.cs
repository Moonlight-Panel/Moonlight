using Microsoft.EntityFrameworkCore;
using MoonCore.Services;
using Moonlight.ApiServer.App.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Moonlight.ApiServer.App.Helpers.Database;

public abstract class DatabaseContext : DbContext
{
    private readonly ConfigService<AppConfiguration> ConfigService;
    public abstract string Prefix { get; }

    public DatabaseContext(ConfigService<AppConfiguration> configService)
    {
        ConfigService = configService;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        
        var config = ConfigService.Get().Database;

        var connectionString = $"host={config.Host};" +
                               $"port={config.Port};" +
                               $"database={config.Database};" +
                               $"uid={config.Username};" +
                               $"pwd={config.Password}";

        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            builder =>
            {
                builder.EnableRetryOnFailure(5);
                builder.SchemaBehavior(MySqlSchemaBehavior.Translate, (name, objectName) => $"{name}_{objectName}");
                builder.MigrationsHistoryTable($"{Prefix}_MigrationHistory");
            }
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Model.SetDefaultSchema(Prefix);
        
        base.OnModelCreating(modelBuilder);
    }
}