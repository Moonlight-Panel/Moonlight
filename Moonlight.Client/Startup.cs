using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MoonCore.Blazor.Services;
using MoonCore.Blazor.Tailwind.Extensions;
using MoonCore.Blazor.Tailwind.Auth;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Permissions;
using Moonlight.Client.Implementations;
using Moonlight.Client.Interfaces;
using Moonlight.Client.Plugins;
using Moonlight.Client.Services;
using Moonlight.Shared.Misc;
using Moonlight.Client.UI;

namespace Moonlight.Client;

public class Startup
{
    private string[] Args;

    // Configuration
    private FrontendConfiguration Configuration;

    // Logging
    private ILoggerProvider[] LoggerProviders;
    private ILoggerFactory LoggerFactory;
    private ILogger<Startup> Logger;

    // WebAssemblyHost
    private WebAssemblyHostBuilder WebAssemblyHostBuilder;
    private WebAssemblyHost WebAssemblyHost;

    // Plugin Loading
    private IPluginStartup[] AdditionalPlugins;
    private IPluginStartup[] PluginStartups;
    private IServiceProvider PluginLoadServiceProvider;

    public async Task Run(string[] args, IPluginStartup[]? additionalPlugins = null)
    {
        Args = args;
        AdditionalPlugins = additionalPlugins ?? [];

        await PrintVersion();
        await SetupLogging();

        await CreateWebAssemblyHostBuilder();

        await LoadConfiguration();
        await InitializePlugins();

        await RegisterLogging();
        await RegisterBase();
        await RegisterAuthentication();
        await HookPluginBuild();

        await BuildWebAssemblyHost();

        await HookPluginConfigure();
        await LoadAssets();

        await WebAssemblyHost.RunAsync();
    }

    private Task PrintVersion()
    {
        // Fancy start console output... yes very fancy :>
        Console.Write("Running ");

        var rainbow = new Crayon.Rainbow(0.5);
        foreach (var c in "Moonlight")
        {
            Console.Write(
                rainbow
                    .Next()
                    .Bold()
                    .Text(c.ToString())
            );
        }

        Console.WriteLine();

        return Task.CompletedTask;
    }

    private async Task LoadConfiguration()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(WebAssemblyHostBuilder.HostEnvironment.BaseAddress);

            var jsonText = await httpClient.GetStringAsync("frontend.json");

            Configuration = JsonSerializer.Deserialize<FrontendConfiguration>(jsonText, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            })!;

            WebAssemblyHostBuilder.Services.AddSingleton(Configuration);
        }
        catch (Exception e)
        {
            Logger.LogCritical("Unable to load configuration. Unable to continue: {e}", e);
            throw;
        }
    }

    private Task RegisterBase()
    {
        WebAssemblyHostBuilder.RootComponents.Add<App>("#app");
        WebAssemblyHostBuilder.RootComponents.Add<HeadOutlet>("head::after");

        WebAssemblyHostBuilder.Services.AddScoped(_ =>
            new HttpClient
            {
                BaseAddress = new Uri(Configuration.ApiUrl)
            }
        );

        WebAssemblyHostBuilder.Services.AddScoped(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            var httpApiClient = new HttpApiClient(httpClient);

            var localStorageService = sp.GetRequiredService<LocalStorageService>();

            httpApiClient.OnConfigureRequest += async request =>
            {
                var accessToken = await localStorageService.GetString("AccessToken");

                if (string.IsNullOrEmpty(accessToken))
                    return;

                request.Headers.Add("Authorization", $"Bearer {accessToken}");
            };

            return httpApiClient;
        });

        WebAssemblyHostBuilder.Services.AddScoped<WindowService>();
        WebAssemblyHostBuilder.Services.AddMoonCoreBlazorTailwind();
        WebAssemblyHostBuilder.Services.AddScoped<LocalStorageService>();

        WebAssemblyHostBuilder.Services.AddScoped<ThemeService>();

        WebAssemblyHostBuilder.Services.AutoAddServices<Program>();

        return Task.CompletedTask;
    }

    #region Asset Loading

    private async Task LoadAssets()
    {
        var jsRuntime = WebAssemblyHost.Services.GetRequiredService<IJSRuntime>();

        foreach (var scriptName in Configuration.Scripts)
            await jsRuntime.InvokeVoidAsync("moonlight.assets.loadJavascript", scriptName);
        
        foreach (var styleName in Configuration.Styles)
            await jsRuntime.InvokeVoidAsync("moonlight.assets.loadStylesheet", styleName);
    }

    #endregion

    #region Plugins

    private Task InitializePlugins()
    {
        // Define minimal service collection
        var startupSc = new ServiceCollection();

        // Create logging proxy
        startupSc.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProviders(LoggerProviders);
        });

        PluginLoadServiceProvider = startupSc.BuildServiceProvider();

        // Collect startups
        var pluginStartups = new List<IPluginStartup>();

        pluginStartups.Add(new CoreStartup());

        pluginStartups.AddRange(AdditionalPlugins); // Used by the development server

        // Do NOT remove the following comment, as its used to place the plugin startup register calls
        // MLBUILD_PLUGIN_STARTUP_HERE


        PluginStartups = pluginStartups.ToArray();

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

    #region Hooks

    private async Task HookPluginBuild()
    {
        foreach (var pluginAppStartup in PluginStartups)
        {
            try
            {
                await pluginAppStartup.BuildApplication(PluginLoadServiceProvider, WebAssemblyHostBuilder);
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
                await pluginAppStartup.ConfigureApplication(PluginLoadServiceProvider, WebAssemblyHost);
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

    #endregion

    #endregion

    #region Logging

    private Task SetupLogging()
    {
        LoggerProviders = LoggerBuildHelper.BuildFromConfiguration(configuration =>
        {
            configuration.Console.Enable = true;
            configuration.Console.EnableAnsiMode = true;
            configuration.FileLogging.Enable = false;
        });

        LoggerFactory = new LoggerFactory();
        LoggerFactory.AddProviders(LoggerProviders);

        Logger = LoggerFactory.CreateLogger<Startup>();

        return Task.CompletedTask;
    }

    private Task RegisterLogging()
    {
        WebAssemblyHostBuilder.Logging.ClearProviders();
        WebAssemblyHostBuilder.Logging.AddProviders(LoggerProviders);

        return Task.CompletedTask;
    }

    #endregion

    #region Web Application

    private Task CreateWebAssemblyHostBuilder()
    {
        WebAssemblyHostBuilder = WebAssemblyHostBuilder.CreateDefault(Args);
        return Task.CompletedTask;
    }

    private Task BuildWebAssemblyHost()
    {
        WebAssemblyHost = WebAssemblyHostBuilder.Build();
        return Task.CompletedTask;
    }

    #endregion

    #region Authentication

    private Task RegisterAuthentication()
    {
        WebAssemblyHostBuilder.Services.AddAuthorizationCore();
        WebAssemblyHostBuilder.Services.AddCascadingAuthenticationState();

        WebAssemblyHostBuilder.Services.AddAuthenticationStateManager<RemoteAuthStateManager>();
        
        WebAssemblyHostBuilder.Services.AddAuthorizationPermissions(options =>
        {
            options.ClaimName = "permissions";
            options.Prefix = "permissions:";
        });

        return Task.CompletedTask;
    }

    #endregion
}