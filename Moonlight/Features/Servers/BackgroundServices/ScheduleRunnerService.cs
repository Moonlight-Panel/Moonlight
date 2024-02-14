using Cronos;
using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Entities.Enums;
using Moonlight.Features.Servers.Services;
using BackgroundService = MoonCore.Abstractions.BackgroundService;

namespace Moonlight.Features.Servers.BackgroundServices;

public class ScheduleRunnerService : BackgroundService
{
    private readonly IServiceProvider ServiceProvider;
    private readonly ConfigService<ConfigV1> ConfigService;

    public ScheduleRunnerService(IServiceProvider serviceProvider, ConfigService<ConfigV1> configService)
    {
        ServiceProvider = serviceProvider;
        ConfigService = configService;
    }

    public override async Task Run()
    {
        var config = ConfigService.Get().Servers.Schedules;
        
        if(config.Disable)
            Logger.Info("Server schedules are disabled");
        
        while (!Cancellation.IsCancellationRequested)
        {
            Logger.Debug("Performing scheduler run");
            
            // Resolve services from di
            using var scope = ServiceProvider.CreateScope();
            var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();
            var scheduleRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerSchedule>>();
            var serverService = scope.ServiceProvider.GetRequiredService<ServerService>();
            
            // Load required data
            var servers = serverRepo
                .Get()
                .Include(x => x.Schedules)
                .Where(x => x.Schedules.Any())
                .ToArray();
            
            Logger.Debug($"Found {servers.Length} servers with schedules");

            foreach (var server in servers)
            {
                foreach (var schedule in server.Schedules)
                {
                    if (!CronExpression.TryParse(schedule.Cron, CronFormat.Standard, out CronExpression cronExpression))
                    {
                        Logger.Warn($"Unable to parse cron expression for schedule '{schedule.Name}' for server '{server.Id}'");
                        continue;
                    }

                    var nextRun = cronExpression.GetNextOccurrence(schedule.LastRunAt, TimeZoneInfo.Utc);

                    if (nextRun == null)
                    {
                        Logger.Warn($"Unable to determine next run time for schedule '{schedule.Name}' for server '{server.Id}'");
                        continue;
                    }

                    // Check if the next run is in the past
                    if (DateTime.UtcNow > nextRun)
                    {
                        var diff = DateTime.UtcNow - nextRun;
                        
                        if(diff.Value.TotalMinutes > 0)
                            Logger.Warn($"Missed executing schedule '{schedule.Name}' for server '{server.Id}'. Was moonlight offline for a while?");
                        else
                            Logger.Warn($"Missed executing schedule '{schedule.Name}' for server '{server.Id}'. The miss difference ({diff.Value.TotalSeconds}s) indicate a miss configuration. Increase your time drift or lower your check delay to fix the error");
                        
                        schedule.LastRunAt = DateTime.UtcNow;
                        schedule.WasLastRunAutomatic = false;
                        
                        scheduleRepo.Update(schedule);
                        continue;
                    }
                    
                    // Check if the next run is too far in the future
                    if ((nextRun - DateTime.UtcNow).Value.TotalSeconds > config.TimeDrift)
                        continue;
                    
                    // Its time to execute the schedule :D
                    await RunSchedule(server, schedule, scope.ServiceProvider);
                    
                    schedule.LastRunAt = DateTime.UtcNow;
                    schedule.WasLastRunAutomatic = true;
                        
                    scheduleRepo.Update(schedule);
                }
            }

            await Task.Delay(config.CheckDelay, Cancellation.Token);
        }
    }

    public async Task RunSchedule(Server server, ServerSchedule schedule, IServiceProvider? provider = null)
    {
        IServiceProvider serviceProvider;

        if (provider != null)
            serviceProvider = provider;
        else
            serviceProvider = ServiceProvider.CreateScope().ServiceProvider;
        
        switch (schedule.ActionType)
        {
            case ScheduleActionType.Power:
                
                //TODO: Implement actual action here
                
                break;
            default:
                Logger.Warn($"Schedule action type {schedule.ActionType} has not been implemented yet");
                break;
        }
    }
}