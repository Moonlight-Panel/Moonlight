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

    public Task Initialize(string[] args, IPluginStartup[]? plugins = null)
    {
        Args = args;
        PluginStartups = plugins ?? [];
        
        return Task.CompletedTask;
    }

    public async Task AddMoonlight(WebApplicationBuilder builder)
    {
        WebApplicationBuilder = builder;

        await PrintVersion();

        await CreateStorage();
        await SetupAppConfiguration();
        await SetupLogging();
        await InitializePlugins();

        await ConfigureKestrel();
        await RegisterAppConfiguration();
        await RegisterLogging();
        await RegisterBase();
        await RegisterDatabase();
        await RegisterAuth();
        await RegisterCors();
        await RegisterHangfire();
        await RegisterSignalR();
        await HookPluginBuild();
    }

    public async Task AddMoonlight(WebApplication application)
    {
        WebApplication = application;

        await PrepareDatabase();

        await UseCors();
        await UseBase();
        await UseAuth();
        await UseHangfire();
        await HookPluginConfigure();

        await MapBase();
        await MapSignalR();
        await HookPluginEndpoints();
    }
}