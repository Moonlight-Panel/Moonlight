using MoonCore.Attributes;
using MoonCore.Helpers;
using BackgroundService = MoonCore.Abstractions.BackgroundService;

namespace Moonlight.Core.Services;

[BackgroundService]
public class StartupJobService : BackgroundService
{
    private readonly List<StartupJob> Jobs = new();
    private readonly IServiceProvider ServiceProvider;
    private readonly ILogger<StartupJobService> Logger;

    public StartupJobService(IServiceProvider serviceProvider, ILogger<StartupJobService> logger)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
    }

    public Task AddJob(string name, TimeSpan delay, Func<IServiceProvider, Task> action)
    {
        Jobs.Add(new()
        {
            Name = name,
            Delay = delay,
            Action = action
        });
        
        return Task.CompletedTask;
    }
    
    public override Task Run()
    {
        Logger.LogInformation("Running startup jobs");
        
        foreach (var job in Jobs)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(job.Delay);
                    await job.Action.Invoke(ServiceProvider);
                }
                catch (Exception e)
                {
                    Logger.LogWarning("The startup job '{name}' failed: {e}", job.Name, e);
                }
            });
        }
        
        return Task.CompletedTask;
    }
    
    struct StartupJob
    {
        public string Name { get; set; }
        public TimeSpan Delay { get; set; }
        public Func<IServiceProvider, Task> Action { get; set; }
    }
}