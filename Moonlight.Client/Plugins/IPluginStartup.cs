using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Moonlight.Client.Plugins;

public interface IPluginStartup
{
    public void AddPlugin(WebAssemblyHostBuilder builder);
    public void ConfigurePlugin(WebAssemblyHost app);
}