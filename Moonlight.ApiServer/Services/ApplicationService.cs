using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoonCore.Attributes;
using MoonCore.Helpers;

namespace Moonlight.ApiServer.Services;

[Singleton]
public class ApplicationService
{
    private ILogger<ApplicationService> Logger;
    private readonly IHost Host;

    public ApplicationService(ILogger<ApplicationService> logger, IHost host)
    {
        Logger = logger;
        Host = host;
    }

    public Task<string> GetOsNameAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows platform detected
            var osVersion = Environment.OSVersion.Version;
            return Task.FromResult($"Windows {osVersion.Major}.{osVersion.Minor}.{osVersion.Build}");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var releaseRaw = File
                .ReadAllLines("/etc/os-release")
                .FirstOrDefault(x => x.StartsWith("PRETTY_NAME="));

            if (string.IsNullOrEmpty(releaseRaw))
                return Task.FromResult("Linux (unknown release)");

            var release = releaseRaw
                .Replace("PRETTY_NAME=", "")
                .Replace("\"", "");
                
            if(string.IsNullOrEmpty(release))
                return Task.FromResult("Linux (unknown release)");

            return Task.FromResult(release);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS platform detected
            var osVersion = Environment.OSVersion.Version;
            return Task.FromResult($"macOS {osVersion.Major}.{osVersion.Minor}.{osVersion.Build}");
        }

        // Unknown platform
        return Task.FromResult("N/A");
    }
    
    public async Task<long> GetMemoryUsageAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var process = Process.GetCurrentProcess();
            return process.PrivateMemorySize64;
        }
        else
        {
            var lines = await File.ReadAllLinesAsync("/proc/self/smaps");
            var kilobytes = 0;

            foreach (var line in lines)
            {
                if(!line.StartsWith("pss:", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var valueString = line
                    .Replace("pss:", "", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("kb", "", StringComparison.InvariantCultureIgnoreCase)
                    .Trim();
                
                kilobytes += int.Parse(valueString);
            }

            return ByteConverter.FromKiloBytes(kilobytes).Bytes;
        }
    }
    
    public Task<TimeSpan> GetUptimeAsync()
    {
        var process = Process.GetCurrentProcess();
        var uptime = DateTime.Now - process.StartTime;
        return Task.FromResult(uptime);
    }

    public Task<int> GetCpuUsageAsync()
    {
        var process = Process.GetCurrentProcess();
        var cpuTime = process.TotalProcessorTime;
        var wallClockTime = DateTime.UtcNow - process.StartTime.ToUniversalTime();
        
        var cpuUsage = (int)(100.0 * cpuTime.TotalMilliseconds / wallClockTime.TotalMilliseconds / Environment.ProcessorCount);
        
        return Task.FromResult(cpuUsage);
    }

    public Task ShutdownAsync()
    {
        Logger.LogCritical("Restart of api server has been requested");

        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            await Host.StopAsync(CancellationToken.None);
        });
        
        return Task.CompletedTask;
    }
}