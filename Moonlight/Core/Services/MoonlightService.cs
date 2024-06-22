using MoonCore.Attributes;
using MoonCore.Helpers;
using Moonlight.Core.Events;

namespace Moonlight.Core.Services;

[Singleton]
public class MoonlightService
{
    public WebApplication Application { get; set; } // Do NOT modify using a plugin

    private readonly DateTime StartTimestamp = DateTime.UtcNow;
    private readonly CoreEvents CoreEvents;
    private readonly ILogger<MoonlightService> Logger;
    
    public MoonlightService(CoreEvents coreEvents, ILogger<MoonlightService> logger)
    {
        CoreEvents = coreEvents;
        Logger = logger;
    }

    public async Task Restart()
    {
        Logger.LogInformation("Restarting moonlight");

        // Notify all users that this instance will restart
        await CoreEvents.OnMoonlightRestart.Invoke();
        
        await Task.Delay(TimeSpan.FromSeconds(3));

        // Stop moonlight so the docker restart policy can restart it
        await Application.StopAsync();
    }

    public Task<TimeSpan> GetUptime()
    {
        return Task.FromResult(DateTime.UtcNow - StartTimestamp);
    }
}