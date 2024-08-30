using Microsoft.EntityFrameworkCore;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Shared.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Moonlight.ApiServer.App.Helpers.Database;

public abstract class DatabaseContext : DbContext
{
    private ConfigService<AppConfiguration> ConfigService;
    public abstract string Prefix { get; }

    public DatabaseContext() // TODO: This
    {
        if (!ExecutionMetadata.IsRunningEf)
        {
            //Console.WriteLine("The Di constructed the database context via the migration constructor. This is a bug in the Di and will be fixed soon");
            
            ConfigService = ExecutionMetadata.ConfigService;
            return;
        }
        
        SetupAsMigrationInstance();
    }

    // This tells the activator utility the di uses to use this constructor
    // https://stackoverflow.com/questions/32931716/dependency-injection-with-multiple-constructors
    [ActivatorUtilitiesConstructor]
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

    protected void SetupAsMigrationInstance()
    {
        // I didnt trust the di to use the marked constructors so meh ¯\_(ツ)_/¯ 
        //Console.WriteLine("Constructed via empty constructor");
        
        ConfigService = new ConfigService<AppConfiguration>(
            PathBuilder.File("storage", "config.json")
        );
    }
}