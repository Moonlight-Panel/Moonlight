using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Moonlight.Client.Interfaces;

public interface IPluginStartup
{
    public Task BuildApplication(WebAssemblyHostBuilder builder);
    public Task ConfigureApplication(WebAssemblyHost app);
}