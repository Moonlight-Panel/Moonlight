using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moonlight.Client.Plugins;
using Moonlight.Client.Services;

namespace Moonlight.Client.Startup;

public partial class Startup
{
    private static void AddPlugins(this WebAssemblyHostBuilder builder, IPluginStartup[] startups)
    {
        foreach (var startup in startups)
            startup.AddPlugin(builder);
        
        // Get all assemblies and combine them into the application assembly service
        // TODO: Consider rewriting this as it may not be that performant to do string checking to find distinct items
        builder.Services.AddSingleton(new ApplicationAssemblyService()
        {
            Assemblies = startups
                .Select(x => x.GetType().Assembly)
                .DistinctBy(x => x.FullName)
                .ToList()
        });
    }

    private static void ConfigurePlugins(this WebAssemblyHost app, IPluginStartup[] startups)
    {
        foreach (var startup in startups)
            startup.ConfigurePlugin(app);
    }
}