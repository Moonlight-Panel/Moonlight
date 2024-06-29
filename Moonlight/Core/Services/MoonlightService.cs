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

    public MoonlightService()
    {
       //TODO: Maybe extract to a method 
        
        if (!File.Exists("version"))
        {
            BuildChannel = "N/A";
            BuildCommitHash = "N/A";
            BuildName = "N/A";
            BuildVersion = "N/A";
            IsDockerRun = false;
            return;
        }

        var line = File.ReadAllText("version");
        var parts = line.Split(";");

        if (parts.Length < 5)
        {
            BuildChannel = "N/A";
            BuildCommitHash = "N/A";
            BuildName = "N/A";
            BuildVersion = "N/A";
            IsDockerRun = false;
        }

        BuildChannel = parts[0];
        BuildCommitHash = parts[1];
        BuildName = parts[2];
        BuildVersion = parts[3];
        IsDockerRun = parts[4] == "docker";
        
        //TODO: Add log call
    }
    
    public async Task Restart()
    {
        Logger.Info("Restarting moonlight");

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