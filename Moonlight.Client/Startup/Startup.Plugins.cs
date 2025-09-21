using Microsoft.Extensions.DependencyInjection;
using MoonCore.Logging;
using Moonlight.Client.Plugins;
using Moonlight.Client.Services;

namespace Moonlight.Client.Startup;

public partial class Startup
{
    private IPluginStartup[] PluginStartups;
    private IServiceProvider PluginLoadServiceProvider;
    
    private Task InitializePluginsAsync()
    {
        // Define minimal service collection
        var startupSc = new ServiceCollection();

        // Create logging proxy
        startupSc.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddAnsiConsole();
        });

        PluginLoadServiceProvider = startupSc.BuildServiceProvider();

        // Add application assembly service
        var appAssemblyService = new ApplicationAssemblyService();

        appAssemblyService.Assemblies.AddRange(
            PluginStartups
                .Select(x => x.GetType().Assembly)
                .Distinct()
        );

        WebAssemblyHostBuilder.Services.AddSingleton(appAssemblyService);

        return Task.CompletedTask;
    }

    private async Task HookPluginBuildAsync()
    {
        foreach (var pluginAppStartup in PluginStartups)
        {
            try
            {
                await pluginAppStartup.BuildApplicationAsync(PluginLoadServiceProvider, WebAssemblyHostBuilder);
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
                await pluginAppStartup.ConfigureApplicationAsync(PluginLoadServiceProvider, WebAssemblyHost);
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
}