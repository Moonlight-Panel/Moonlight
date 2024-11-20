using Microsoft.EntityFrameworkCore;
using Moonlight.ApiServer.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Moonlight.ApiServer.Helpers;

public abstract class DatabaseContext : DbContext
{
    public abstract string Prefix { get; }
    
    private readonly AppConfiguration Configuration;

    public DatabaseContext(AppConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var config = Configuration.Database;

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