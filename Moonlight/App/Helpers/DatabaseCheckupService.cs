using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Services;
using Moonlight.App.Services.Files;
using MySql.Data.MySqlClient;

namespace Moonlight.App.Helpers;

public class DatabaseCheckupService
{
    private readonly ConfigService ConfigService;

    public DatabaseCheckupService(ConfigService configService)
    {
        ConfigService = configService;
    }

    public async Task Perform()
    {
        var context = new DataContext(ConfigService);

        Logger.Info("Checking database");
        
        if (!await context.Database.CanConnectAsync())
        {
            Logger.Fatal("-----------------------------------------------");
            Logger.Fatal("Unable to connect to mysql database");
            Logger.Fatal("Please make sure the configuration is correct");
            Logger.Fatal("");
            Logger.Fatal("Moonlight will wait 1 minute, then exit");
            Logger.Fatal("-----------------------------------------------");
            
            Thread.Sleep(TimeSpan.FromMinutes(1));
            Environment.Exit(10324);
        }

        Logger.Info("Checking for pending migrations");

        var migrations = (await context.Database
            .GetPendingMigrationsAsync())
            .ToArray();

        if (migrations.Any())
        {
            Logger.Info($"{migrations.Length} migrations pending. Updating now");

            try
            {
                var backupHelper = new BackupHelper();
                await backupHelper.CreateBackup(
                    PathBuilder.File("storage", "backups", $"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()}.zip"));
            }
            catch (Exception e)
            {
                Logger.Fatal("Unable to create backup");
                Logger.Fatal(e);
                Logger.Fatal("Moonlight will continue to start and apply the migrations without a backup");
            }
            
            Logger.Info("Applying migrations");
            
            await context.Database.MigrateAsync();
            
            Logger.Info("Successfully applied migrations");
        }
        else
        {
            Logger.Info("Database is up-to-date. No migrations have been performed");
        }
    }
}