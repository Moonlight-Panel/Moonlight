using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonCore.Logging;
using Moonlight.ApiServer.Plugins;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private IServiceProvider PluginLoadServiceProvider;
    private IPluginStartup[] PluginStartups;
    
    private Task InitializePlugins()
    {
        // Create service provider for starting up
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton(Configuration);

        serviceCollection.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddAnsiConsole();
        });

        PluginLoadServiceProvider = serviceCollection.BuildServiceProvider();
        
        return Task.CompletedTask;
    }

    private async Task HookPluginBuild()
    {
        foreach (var pluginAppStartup in PluginStartups)
        {
            try
            {
                await pluginAppStartup.BuildApplication(PluginLoadServiceProvider, WebApplicationBuilder);
            }
            catch (Exception e)
            {
                Logger.LogError(
                    "An error occured while processing 'BuildApp' for '{name}': {e}",
                    pluginAppStartup.GetType().FullName,
                    e
                );
            }
        }
    }

    private async Task HookPluginConfigure()
    {
        foreach (var pluginAppStartup in PluginStartups)
        {
            try
            {
                await pluginAppStartup.ConfigureApplication(PluginLoadServiceProvider, WebApplication);
            }
            catch (Exception e)
            {
                Logger.LogError(
                    "An error occured while processing 'ConfigureApp' for '{name}': {e}",
                    pluginAppStartup.GetType().FullName,
                    e
                );
            }
        }
    }

    private async Task HookPluginEndpoints()
    {
        foreach (var pluginEndpointStartup in PluginStartups)
        {
            try
            {
                await pluginEndpointStartup.ConfigureEndpoints(PluginLoadServiceProvider, WebApplication);
            }
            catch (Exception e)
            {
                Logger.LogError(
                    "An error occured while processing 'ConfigureEndpoints' for '{name}': {e}",
                    pluginEndpointStartup.GetType().FullName,
                    e
                );
            }
        }
    }
}