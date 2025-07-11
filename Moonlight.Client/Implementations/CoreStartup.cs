using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moonlight.Client.Interfaces;
using Moonlight.Client.Plugins;

namespace Moonlight.Client.Implementations;

public class CoreStartup : IPluginStartup
{
    public Task BuildApplication(IServiceProvider serviceProvider, WebAssemblyHostBuilder builder)
    {
        builder.Services.AddSingleton<ISidebarItemProvider, DefaultSidebarItemProvider>();
        builder.Services.AddSingleton<IOverviewElementProvider, DefaultOverviewElementProvider>();
        
        return Task.CompletedTask;
    }

    public Task ConfigureApplication(IServiceProvider serviceProvider, WebAssemblyHost app)
        => Task.CompletedTask;
}