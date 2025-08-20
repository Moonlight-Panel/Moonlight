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
    
    
    public Task Initialize(IPluginStartup[]? plugins = null)
    {
        PluginStartups = plugins ?? [];
        
        return Task.CompletedTask;
    }

    public async Task AddMoonlight(WebAssemblyHostBuilder builder)
    {
        WebAssemblyHostBuilder = builder;
        
        await PrintVersion();

        await SetupLogging();
        await LoadConfiguration();
        await InitializePlugins();

        await RegisterLogging();
        await RegisterBase();
        await RegisterAuthentication();
        await HookPluginBuild();
    }
    
    public async Task AddMoonlight(WebAssemblyHost assemblyHost)
    {
        WebAssemblyHost = assemblyHost;
        
        await HookPluginConfigure();
    }
}