using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moonlight.Client.Interfaces;
using Moonlight.Client.Plugins;

namespace Moonlight.Client.Implementations;

public class CoreStartup : IPluginStartup
{
    public Task BuildApplicationAsync(IServiceProvider serviceProvider, WebAssemblyHostBuilder builder)
    {
        builder.Services.AddSingleton<ISidebarItemProvider, DefaultSidebarItemProvider>();
        builder.Services.AddSingleton<IOverviewElementProvider, DefaultOverviewElementProvider>();
        
        return Task.CompletedTask;
    }

    public Task ConfigureApplicationAsync(IServiceProvider serviceProvider, WebAssemblyHost app)
        => Task.CompletedTask;
}