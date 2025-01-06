using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MoonCore.Blazor.Extensions;
using MoonCore.Blazor.Services;
using MoonCore.Blazor.Tailwind.Extensions;
using MoonCore.Blazor.Tailwind.Forms;
using MoonCore.Blazor.Tailwind.Forms.Components;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.PluginFramework.Extensions;
using MoonCore.Plugins;
using Moonlight.Client.Implementations;
using Moonlight.Client.Interfaces;
using Moonlight.Client.Services;
using Moonlight.Client.UI;
using Moonlight.Client.UI.Forms;
using Moonlight.Shared.Misc;

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
    private PluginLoaderService PluginLoaderService;
    private ApplicationAssemblyService ApplicationAssemblyService;

    private IAppStartup[] PluginAppStartups;

    public async Task Run(string[] args, Assembly[]? assemblies = null)
    {
        Args = args;

        // Setup assembly storage
        ApplicationAssemblyService = new()
        {
            AdditionalAssemblies = assemblies ?? []
        };

        await PrintVersion();
        await SetupLogging();

        await CreateWebAssemblyHostBuilder();

        await LoadConfiguration();
        await LoadPlugins();
        await InitializePlugins();

        await RegisterLogging();
        await RegisterBase();
        await RegisterOAuth2();
        await RegisterFormComponents();
        await RegisterInterfaces();
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

        WebAssemblyHostBuilder.Services.AddScoped<WindowService>();
        WebAssemblyHostBuilder.Services.AddScoped<DownloadService>();
        WebAssemblyHostBuilder.Services.AddMoonCoreBlazorTailwind();
        WebAssemblyHostBuilder.Services.AddScoped<LocalStorageService>();

        WebAssemblyHostBuilder.Services.AutoAddServices<Program>();

        return Task.CompletedTask;
    }

    private Task RegisterOAuth2()
    {
        WebAssemblyHostBuilder.AddTokenAuthentication();
        WebAssemblyHostBuilder.AddOAuth2();

        return Task.CompletedTask;
    }

    private Task RegisterFormComponents()
    {
        FormComponentRepository.Set<string, StringComponent>();
        FormComponentRepository.Set<int, IntComponent>();
        FormComponentRepository.Set<DateTime, DateComponent>();

        return Task.CompletedTask;
    }

    #region Asset Loading

    private async Task LoadAssets()
    {
        var jsRuntime = WebAssemblyHost.Services.GetRequiredService<IJSRuntime>();

        foreach (var scriptName in Configuration.Scripts)
            await jsRuntime.InvokeVoidAsync("moonlight.assets.loadJavascript", scriptName);
    }

    #endregion

    #region Interfaces

    private Task RegisterInterfaces()
    {
        WebAssemblyHostBuilder.Services.AddInterfaces(configuration =>
        {
            // We use moonlight itself as a plugin assembly
            configuration.AddAssembly(typeof(Startup).Assembly);

            configuration.AddAssemblies(ApplicationAssemblyService.AdditionalAssemblies);
            configuration.AddAssemblies(ApplicationAssemblyService.PluginAssemblies);

            configuration.AddInterface<IAppLoader>();
            configuration.AddInterface<IAppScreen>();
            configuration.AddInterface<ISidebarItemProvider>();
        });

        return Task.CompletedTask;
    }

    #endregion

    #region Plugins

    private async Task LoadPlugins()
    {
        // Initialize api server plugin loader
        PluginLoaderService = new PluginLoaderService(
            LoggerFactory.CreateLogger<PluginLoaderService>()
        );

        // Create everything required to stream plugins
        using var clientForStreaming = new HttpClient();

        clientForStreaming.BaseAddress = new Uri(Configuration.HostEnvironment == "ApiServer"
            ? Configuration.ApiUrl
            : WebAssemblyHostBuilder.HostEnvironment.BaseAddress
        );

        PluginLoaderService.AddSource(new RemotePluginSource(
            Configuration,
            LoggerFactory.CreateLogger<RemotePluginSource>(),
            clientForStreaming
        ));

        // Perform assembly loading
        await PluginLoaderService.Load();

        // Add plugin loader service to di for the Router/App.razor
        ApplicationAssemblyService.PluginAssemblies = PluginLoaderService.PluginAssemblies;

        WebAssemblyHostBuilder.Services.AddSingleton(ApplicationAssemblyService);
    }

    private Task InitializePlugins()
    {
        var initialisationServiceCollection = new ServiceCollection();

        initialisationServiceCollection.AddLogging(builder => { builder.AddProviders(LoggerProviders); });

        // Configure plugin loading by using the interface service
        initialisationServiceCollection.AddInterfaces(configuration =>
        {
            // We use moonlight itself as a plugin assembly
            configuration.AddAssembly(typeof(Startup).Assembly);

            configuration.AddAssemblies(ApplicationAssemblyService.AdditionalAssemblies);
            configuration.AddAssemblies(ApplicationAssemblyService.PluginAssemblies);

            configuration.AddInterface<IAppStartup>();
        });

        var initialisationServiceProvider = initialisationServiceCollection.BuildServiceProvider();

        PluginAppStartups = initialisationServiceProvider.GetRequiredService<IAppStartup[]>();

        return Task.CompletedTask;
    }

    #region Hooks

    private async Task HookPluginBuild()
    {
        foreach (var pluginAppStartup in PluginAppStartups)
        {
            try
            {
                await pluginAppStartup.BuildApp(WebAssemblyHostBuilder);
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
        foreach (var pluginAppStartup in PluginAppStartups)
        {
            try
            {
                await pluginAppStartup.ConfigureApp(WebAssemblyHost);
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
}