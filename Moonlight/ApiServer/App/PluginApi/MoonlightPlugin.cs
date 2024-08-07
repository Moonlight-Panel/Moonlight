using MoonCore.Extended.Helpers;

namespace Moonlight.ApiServer.App.PluginApi;

public abstract class MoonlightPlugin
{
    public ILogger Logger { get; set; }
    public PluginService PluginService { get; set; }
    
    public MoonlightPlugin(ILogger logger, PluginService pluginService)
    {
        Logger = logger;
        PluginService = pluginService;
    }
    
    public virtual Task OnLoaded() => Task.CompletedTask;
    public virtual Task OnAppBuilding(WebApplicationBuilder builder, DatabaseHelper databaseHelper) => Task.CompletedTask;
    public virtual Task OnAppConfiguring(WebApplication app) => Task.CompletedTask;
}