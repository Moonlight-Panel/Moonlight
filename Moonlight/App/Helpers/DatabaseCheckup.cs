using System.Diagnostics;
using Logging.Net;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Services;
using Moonlight.App.Services.Files;
using MySql.Data.MySqlClient;

namespace Moonlight.App.Helpers;

public class DatabaseCheckup
{
    public static void Perform()
    {
        var context = new DataContext(new ConfigService(new StorageService()));

        Logger.Info("Checking database");

        Logger.Info("Checking for pending migrations");

        var migrations = context.Database
            .GetPendingMigrations()
            .ToArray();
        
        if (migrations.Any())
        {
            Logger.Info($"{migrations.Length} migrations pending. Updating now");
            
            BackupDatabase();
            
            Logger.Info("Applying migrations");
            
            context.Database.Migrate();
            
            Logger.Info("Successfully applied migrations");
        }
        else
        {
            Logger.Info("Database is up-to-date. No migrations have been performed");
        }
    }

    public static void BackupDatabase()
    {
        Logger.Info("Creating backup from database");
        
        var configService = new ConfigService(new StorageService());
        var dateTimeService = new DateTimeService();
        
        var config = configService
            .GetSection("Moonlight")
            .GetSection("Database");

        var connectionString = $"host={config.GetValue<string>("Host")};" +
                               $"port={config.GetValue<int>("Port")};" +
                               $"database={config.GetValue<string>("Database")};" +
                               $"uid={config.GetValue<string>("Username")};" +
                               $"pwd={config.GetValue<string>("Password")}";
        
        string file = PathBuilder.File("storage", "backups", $"{dateTimeService.GetCurrentUnix()}-mysql.sql");
        
        Logger.Info($"Saving it to: {file}");
        Logger.Info("Starting backup...");

        var sw = new Stopwatch();
        sw.Start();

        using MySqlConnection conn = new MySqlConnection(connectionString);
        using MySqlCommand cmd = new MySqlCommand();
        using MySqlBackup mb = new MySqlBackup(cmd);
        
        cmd.Connection = conn;
        conn.Open();
        mb.ExportToFile(file);
        conn.Close();

        sw.Stop();
        Logger.Info($"Done. {sw.Elapsed.TotalSeconds}s");
    }
}