using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonCore.Logging;
using Moonlight.ApiServer.Plugins;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private IServiceProvider PluginLoadServiceProvider;
    private IPluginStartup[] PluginStartups;
    
    private Task InitializePluginsAsync()
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

    private async Task HookPluginBuildAsync()
    {
        foreach (var pluginAppStartup in PluginStartups)
        {
            try
            {
                await pluginAppStartup.BuildApplicationAsync(PluginLoadServiceProvider, WebApplicationBuilder);
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

    private async Task HookPluginConfigureAsync()
    {
        foreach (var pluginAppStartup in PluginStartups)
        {
            try
            {
                await pluginAppStartup.ConfigureApplicationAsync(PluginLoadServiceProvider, WebApplication);
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

    private async Task HookPluginEndpointsAsync()
    {
        foreach (var pluginEndpointStartup in PluginStartups)
        {
            try
            {
                await pluginEndpointStartup.ConfigureEndpointsAsync(PluginLoadServiceProvider, WebApplication);
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