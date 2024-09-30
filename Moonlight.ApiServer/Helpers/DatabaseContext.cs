using Microsoft.EntityFrameworkCore;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Moonlight.ApiServer.Helpers;

public abstract class DatabaseContext : DbContext
{
    private ConfigService<AppConfiguration>? ConfigService;
    public abstract string Prefix { get; }

    public DatabaseContext()
    {
        ConfigService = ApplicationStateHelper.Configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        // If no config service has been configured, we are probably
        // in a EF Core migration, so we need to construct the config manually
        if (ConfigService == null)
        {
            ConfigService = new ConfigService<AppConfiguration>(
                PathBuilder.File("storage", "config.json")
            );
        }

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