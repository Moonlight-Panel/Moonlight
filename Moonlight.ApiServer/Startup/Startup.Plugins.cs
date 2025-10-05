using Microsoft.AspNetCore.Builder;
using Moonlight.ApiServer.Plugins;

namespace Moonlight.ApiServer.Startup;

public static partial class Startup
{
    private static void AddPlugins(this WebApplicationBuilder builder, IPluginStartup[] startups)
    {
        foreach (var startup in startups)
            startup.AddPlugin(builder);
    }

    private static void UsePlugins(this WebApplication application, IPluginStartup[] startups)
    {
        foreach (var startup in startups)
            startup.UsePlugin(application);
    }

    private static void MapPlugins(this WebApplication application, IPluginStartup[] startups)
    {
        foreach (var startup in startups)
            startup.MapPlugin(application);
    }
}