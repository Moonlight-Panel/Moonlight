using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Plugins;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private string[] Args;

    // Logger
    public ILogger<Startup> Logger { get; private set; }

    // Configuration
    public AppConfiguration Configuration { get; private set; }

    // WebApplication Stuff
    public WebApplication WebApplication { get; private set; }
    public WebApplicationBuilder WebApplicationBuilder { get; private set; }

    public Task InitializeAsync(string[] args, IPluginStartup[]? plugins = null)
    {
        Args = args;
        PluginStartups = plugins ?? [];
        
        return Task.CompletedTask;
    }

    public async Task AddMoonlightAsync(WebApplicationBuilder builder)
    {
        WebApplicationBuilder = builder;

        await PrintVersionAsync();

        await CreateStorageAsync();
        await SetupAppConfigurationAsync();
        await SetupLoggingAsync();
        await InitializePluginsAsync();

        await ConfigureKestrelAsync();
        await RegisterAppConfigurationAsync();
        await RegisterLoggingAsync();
        await RegisterBaseAsync();
        await RegisterDatabaseAsync();
        await RegisterAuthAsync();
        await RegisterCorsAsync();
        await RegisterHangfireAsync();
        await RegisterSignalRAsync();
        await HookPluginBuildAsync();
    }

    public async Task AddMoonlightAsync(WebApplication application)
    {
        WebApplication = application;

        await PrepareDatabaseAsync();

        await UseCorsAsync();
        await UseBaseAsync();
        await UseAuthAsync();
        await UseHangfireAsync();
        await HookPluginConfigureAsync();

        await MapBaseAsync();
        await MapSignalRAsync();
        await HookPluginEndpointsAsync();
    }
}