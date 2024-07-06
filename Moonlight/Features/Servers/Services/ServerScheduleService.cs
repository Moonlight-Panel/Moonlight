using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Helpers;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Models.Abstractions;
using Newtonsoft.Json;

namespace Moonlight.Features.Servers.Services;

[Singleton]
public class ServerScheduleService
{
    private readonly IServiceProvider ServiceProvider;
    public readonly Dictionary<string, ScheduleAction> Actions = new();
    private readonly ILogger<ServerScheduleService> Logger;

    public ServerScheduleService(IServiceProvider serviceProvider, ILogger<ServerScheduleService> logger)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
    }

    public Task RegisterAction<T>(string id) where T : ScheduleAction
    {
        if (Actions.ContainsKey(id))
            return Task.CompletedTask;

        var action = Activator.CreateInstance<T>();
        var actionTyped = action! as ScheduleAction;

        Actions.Add(id, actionTyped!);

        return Task.CompletedTask;
    }

    public async Task<ScheduleRunResult> Run(Server s, ServerSchedule sh)
    {
        using var scope = ServiceProvider.CreateScope();

        var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();
        var scheduleRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerSchedule>>();

        var server = serverRepo.Get().First(x => x.Id == s.Id);
        var schedule = scheduleRepo.Get().Include(x => x.Items).First(x => x.Id == sh.Id);

        var sw = new Stopwatch();
        sw.Start();

        foreach (var scheduleItem in schedule.Items.OrderBy(x => x.Priority).ToArray())
        {
            if (!Actions.ContainsKey(scheduleItem.Action))
            {
                Logger.LogWarning("The server {serverId} has a invalid action type '{action}'", server.Id, scheduleItem.Action);
                continue;
            }

            var action = Actions.First(x => x.Key == scheduleItem.Action).Value;

            try
            {
                object config;

                if (action.FormType == typeof(object))
                    config = new();
                else
                    config = JsonConvert.DeserializeObject(scheduleItem.DataJson, action.FormType)!;

                await action.Execute(server, config, scope.ServiceProvider);
            }
            catch (Exception e)
            {
                Logger.LogWarning("An unhandled error occured while running schedule {name} for server {serverId}: {e}", schedule.Name, server.Id, e);
                
                sw.Stop();

                return new()
                {
                    Failed = true,
                    ExecutionSeconds = (int)sw.Elapsed.TotalSeconds
                };
            }
        }
        
        sw.Stop();

        return new()
        {
            Failed = false,
            ExecutionSeconds = (int)sw.Elapsed.TotalSeconds
        };
    }
}