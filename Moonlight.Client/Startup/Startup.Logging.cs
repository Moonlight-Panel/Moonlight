using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MoonCore.Logging;

namespace Moonlight.Client.Startup;

public static partial class Startup
{
    private static void AddLogging(this WebAssemblyHostBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddAnsiConsole();
    }
}