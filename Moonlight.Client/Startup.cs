using System.Reflection;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MoonCore.Blazor.Extensions;
using MoonCore.Blazor.Services;
using MoonCore.Blazor.Tailwind.Extensions;
using MoonCore.Blazor.Tailwind.Forms;
using MoonCore.Blazor.Tailwind.Forms.Components;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.PluginFramework.Extensions;
using MoonCore.Plugins;
using Moonlight.Client.Interfaces;
using Moonlight.Client.Services;
using Moonlight.Client.UI;
using Moonlight.Client.UI.Forms;

namespace Moonlight.Client;

public class Startup
{
    private string[] Args;
    private Assembly[] AdditionalAssemblies;
    
    // Logging
    private ILoggerProvider[] LoggerProviders;
    private ILoggerFactory LoggerFactory;
    private ILogger<Startup> Logger;
    
    // WebAssemblyHost
    private WebAssemblyHostBuilder WebAssemblyHostBuilder;
    private WebAssemblyHost WebAssemblyHost;
    
    // Plugin Loading
    private PluginLoaderService PluginLoaderService;
    
    public async Task Run(string[] args, Assembly[]? assemblies = null)
    {
        Args = args;
        AdditionalAssemblies = assemblies ?? [];

        await PrintVersion();
        await SetupLogging();

        await CreateWebAssemblyHostBuilder();

        await LoadPlugins();
        
        await RegisterLogging();
        await RegisterBase();
        await RegisterOAuth2();
        await RegisterFormComponents();
        await RegisterInterfaces();

        await BuildWebAssemblyHost();

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
    
    private Task RegisterBase()
    {
        WebAssemblyHostBuilder.RootComponents.Add<App>("#app");
        WebAssemblyHostBuilder.RootComponents.Add<HeadOutlet>("head::after");

        WebAssemblyHostBuilder.Services.AddScoped(_ =>
            new HttpClient
            {
                BaseAddress = new Uri(WebAssemblyHostBuilder.HostEnvironment.BaseAddress)
            }
        );
        
        WebAssemblyHostBuilder.Services.AddScoped<WindowService>();
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
        
        return Task.CompletedTask;
    }

    #region Interfaces

    private Task RegisterInterfaces()
    {
        WebAssemblyHostBuilder.Services.AddInterfaces(configuration =>
        {
            // We use moonlight itself as a plugin assembly
            configuration.AddAssembly(typeof(Startup).Assembly);
            
            configuration.AddAssemblies(AdditionalAssemblies);
            configuration.AddAssemblies(PluginLoaderService.PluginAssemblies);

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

        // Build source from the retrieved data
        var pluginsStreamUrl = $"{WebAssemblyHostBuilder.HostEnvironment.BaseAddress}api/pluginsStream";
        PluginLoaderService.AddHttpHostedSource(pluginsStreamUrl);

        // Perform assembly loading
        await PluginLoaderService.Load();
        
        // Add plugin loader service to di for the Router/App.razor
        WebAssemblyHostBuilder.Services.AddSingleton(PluginLoaderService);
    }

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