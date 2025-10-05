using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moonlight.Client.Interfaces;
using Moonlight.Client.Plugins;

namespace Moonlight.Client.Implementations;

public class CoreStartup : IPluginStartup
{
    public void AddPlugin(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddSingleton<ISidebarItemProvider, DefaultSidebarItemProvider>();
        builder.Services.AddSingleton<IOverviewElementProvider, DefaultOverviewElementProvider>();
    }

    public void ConfigurePlugin(WebAssemblyHost app)
    {
        
    }
}