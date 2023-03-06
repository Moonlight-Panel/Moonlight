using System.Runtime.InteropServices;
using Logging.Net;

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
}