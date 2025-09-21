using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Moonlight.Client.Plugins;

public interface IPluginStartup
{
    public Task BuildApplicationAsync(IServiceProvider serviceProvider, WebAssemblyHostBuilder builder);
    public Task ConfigureApplicationAsync(IServiceProvider serviceProvider, WebAssemblyHost app);
}