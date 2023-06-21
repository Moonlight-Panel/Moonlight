using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Moonlight.App.Helpers;

public class HostSystemHelper
{
    public string GetOsName()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows platform detected
                var osVersion = Environment.OSVersion.Version;
                return $"Windows {osVersion.Major}.{osVersion.Minor}.{osVersion.Build}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux platform detected
                var release = System.IO.File.ReadAllText("/etc/os-release");
                var version = release.Split('\n')
                    .FirstOrDefault(x => x.StartsWith("VERSION_ID="))
                    ?.Split('=')[1]
                    ?.Replace("\"", "");
                return $"Linux {version ?? release}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS platform detected
                var osVersion = Environment.OSVersion.Version;
                return $"macOS {osVersion.Major}.{osVersion.Minor}.{osVersion.Build}";
            }
            else
            {
                // Unknown platform
                return "N/A";
            }
        }
        catch (Exception e)
        {
            Logger.Warn("Error retrieving os information");
            Logger.Warn(e);

            return "N/A";
        }
    }

    public int GetMemoryUsage()
    {
        var process = Process.GetCurrentProcess();
        var bytes = process.WorkingSet64;
        return (int)(bytes / (1024.0 * 1024.0));
    }

    public int GetCpuUsage()
    {
        var process = Process.GetCurrentProcess();
        var cpuTime = process.TotalProcessorTime;
        var wallClockTime = DateTime.UtcNow - process.StartTime.ToUniversalTime();
        return (int)(100.0 * cpuTime.TotalMilliseconds / wallClockTime.TotalMilliseconds / Environment.ProcessorCount);
    }
}