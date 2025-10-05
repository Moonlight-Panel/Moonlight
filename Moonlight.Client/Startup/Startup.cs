using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Moonlight.Client.Plugins;

namespace Moonlight.Client.Startup;

public static partial class Startup
{
    public static void AddMoonlight(this WebAssemblyHostBuilder builder, IPluginStartup[] startups)
    {
        PrintVersion();

        builder.AddLogging();
        builder.AddBase();
        builder.AddAuth();
        builder.AddPlugins(startups);
    }

    public static void ConfigureMoonlight(this WebAssemblyHost app, IPluginStartup[] startups)
    {
        app.ConfigurePlugins(startups);
    }
}