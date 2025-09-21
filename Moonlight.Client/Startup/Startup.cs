using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Moonlight.Client.Plugins;
using Moonlight.Shared.Misc;

namespace Moonlight.Client.Startup;

public partial class Startup
{
    public ILogger<Startup> Logger { get; private set; }

    // WebAssemblyHost
    public WebAssemblyHostBuilder WebAssemblyHostBuilder { get; private set; }
    public WebAssemblyHost WebAssemblyHost { get; private set; }

    // Configuration
    public FrontendConfiguration Configuration { get; private set; }
    
    
    public Task InitializeAsync(IPluginStartup[]? plugins = null)
    {
        PluginStartups = plugins ?? [];
        
        return Task.CompletedTask;
    }

    public async Task AddMoonlightAsync(WebAssemblyHostBuilder builder)
    {
        WebAssemblyHostBuilder = builder;
        
        await PrintVersionAsync();

        await SetupLoggingAsync();
        await LoadConfigurationAsync();
        await InitializePluginsAsync();

        await RegisterLoggingAsync();
        await RegisterBaseAsync();
        await RegisterAuthenticationAsync();
        await HookPluginBuildAsync();
    }
    
    public async Task AddMoonlightAsync(WebAssemblyHost assemblyHost)
    {
        WebAssemblyHost = assemblyHost;
        
        await HookPluginConfigureAsync();
    }
}