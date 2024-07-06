using MoonCore.Attributes;
using MoonCore.Helpers;
using Moonlight.Core.Events;

namespace Moonlight.Core.Services;

[Singleton]
public class MoonlightService
{
    public readonly string BuildChannel;
    public readonly string BuildCommitHash;
    public readonly string BuildName;
    public readonly string BuildVersion;
    public readonly bool IsDockerRun;
    
    public WebApplication Application { get; set; } // Do NOT modify using a plugin

    private readonly DateTime StartTimestamp = DateTime.UtcNow;
    private readonly CoreEvents CoreEvents;
    private readonly ILogger<MoonlightService> Logger;

    public MoonlightService(CoreEvents coreEvents, ILogger<MoonlightService> logger)
    {
        CoreEvents = coreEvents;
        Logger = logger;
        
       //TODO: Maybe extract to a method to make this a bit cleaner
       if (File.Exists("version"))
       {
           var line = File.ReadAllText("version");
           line = line.Trim();
           
           var parts = line.Split(";");

           if (parts.Length >= 5)
           {
               BuildChannel = parts[0];
               BuildCommitHash = parts[1];
               BuildName = parts[2];
               BuildVersion = parts[3];
               IsDockerRun = parts[4] == "docker";
               return;
           }
       }
       
       BuildChannel = "N/A";
       BuildCommitHash = "N/A";
       BuildName = "N/A";
       BuildVersion = "N/A";
       IsDockerRun = false;
        
        //TODO: Add log call
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