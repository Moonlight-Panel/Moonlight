using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Moonlight.Client.Interfaces;

namespace Moonlight.Client.Implementations;

public class CoreStartup : IPluginStartup
{
    public Task BuildApplication(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddSingleton<ISidebarItemProvider, DefaultSidebarItemProvider>();
        builder.Services.AddSingleton<IOverviewElementProvider, DefaultOverviewElementProvider>();
        
        return Task.CompletedTask;
    }

    public Task ConfigureApplication(WebAssemblyHost app)
        => Task.CompletedTask;
}