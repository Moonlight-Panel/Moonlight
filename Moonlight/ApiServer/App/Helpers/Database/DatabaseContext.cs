using Microsoft.EntityFrameworkCore;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Shared.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Moonlight.ApiServer.App.Helpers.Database;

public abstract class DatabaseContext : DbContext
{
    private readonly ConfigService<AppConfiguration> ConfigService;
    public abstract string Prefix { get; }

    // Because we need to handle ef migrations without di
    // we cannot use di to inject the config service
    public DatabaseContext()
    {
        if (ApplicationContext.IsRunningEfMigration)
        {
            ConfigService = new ConfigService<AppConfiguration>(
                PathBuilder.File("storage", "config.json")
            );
        }
        else
            ConfigService = ApplicationContext.ConfigService;
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