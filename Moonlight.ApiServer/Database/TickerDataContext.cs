using Microsoft.EntityFrameworkCore;
using MoonCore.Extended.SingleDb;
using Moonlight.ApiServer.Configuration;
using TickerQ.EntityFrameworkCore.Configurations;

namespace Moonlight.ApiServer.Database;

public class TickerDataContext : DatabaseContext
{
    public override string Prefix => "Ticker";
    
    public TickerDataContext(AppConfiguration configuration)
    {
        Options = new()
        {
            Host = configuration.Database.Host,
            Port = configuration.Database.Port,
            Username = configuration.Database.Username,
            Password = configuration.Database.Password,
            Database = configuration.Database.Database
        };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply TickerQ entity configurations explicitly
        modelBuilder.ApplyConfiguration(new TimeTickerConfigurations()); 
        modelBuilder.ApplyConfiguration(new CronTickerConfigurations()); 
        modelBuilder.ApplyConfiguration(new CronTickerOccurrenceConfigurations()); 
    }
}