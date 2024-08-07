using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Moonlight.Client.App.PluginApi;

public class MoonlightClientPlugin
{
    public ILogger Logger { get; set; }
    public PluginService PluginService { get; set; }
    
    public MoonlightClientPlugin(ILogger logger, PluginService pluginService)
    {
        Logger = logger;
        PluginService = pluginService;
    }
    
    public virtual Task OnLoaded() => Task.CompletedTask;
    public virtual Task OnAppBuilding(WebAssemblyHostBuilder builder) => Task.CompletedTask;
    public virtual Task OnAppConfiguring(WebAssemblyHost app) => Task.CompletedTask;
}