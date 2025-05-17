using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Moonlight.Client.Plugins;

public interface IPluginStartup
{
    public Task BuildApplication(IServiceProvider serviceProvider, WebAssemblyHostBuilder builder);
    public Task ConfigureApplication(IServiceProvider serviceProvider, WebAssemblyHost app);
}