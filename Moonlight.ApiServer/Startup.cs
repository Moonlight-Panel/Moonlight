using System.Reflection;
using System.Text.Json;
using MoonCore.Configuration;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Extensions;
using MoonCore.Extended.Helpers;
using MoonCore.Extended.OAuth2.Consumer;
using MoonCore.Extended.OAuth2.Consumer.Extensions;
using MoonCore.Extended.OAuth2.LocalProvider;
using MoonCore.Extended.OAuth2.LocalProvider.Extensions;
using MoonCore.Extended.OAuth2.LocalProvider.Implementations;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.PluginFramework.Extensions;
using MoonCore.Plugins;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Http.Middleware;
using Moonlight.ApiServer.Implementations.OAuth2;
using Moonlight.ApiServer.Interfaces.Auth;
using Moonlight.ApiServer.Interfaces.OAuth2;
using Moonlight.ApiServer.Interfaces.Startup;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer;

// Cry about it
#pragma warning disable ASP0000

public class Startup
{
    private string[] Args;
    private Assembly[] AdditionalAssemblies;

    // Logging
    private ILoggerProvider[] LoggerProviders;
    private ILoggerFactory LoggerFactory;
    private ILogger<Startup> Logger;

    // Configuration
    private AppConfiguration Configuration;
    private ConfigurationService ConfigurationService;
    private ConfigurationOptions ConfigurationOptions;

    // WebApplication Stuff
    private WebApplication WebApplication;
    private WebApplicationBuilder WebApplicationBuilder;

    // Plugin Loading
    private PluginService PluginService;
    private PluginLoaderService PluginLoaderService;

    private IAppStartup[] PluginAppStartups;
    private IDatabaseStartup[] PluginDatabaseStartups;
    private IEndpointStartup[] PluginEndpointStartups;

    public async Task Run(string[] args, Assembly[]? additionalAssemblies = null)
    {
        Args = args;
        AdditionalAssemblies = additionalAssemblies ?? [];

        await PrintVersion();

        await CreateStorage();
        await SetupAppConfiguration();
        await SetupLogging();
        await LoadPlugins();
        await InitializePlugins();

        await CreateWebApplicationBuilder();

        await RegisterAppConfiguration();
        await RegisterLogging();
        await RegisterBase();
        await RegisterDatabase();
        await RegisterOAuth2();
        await RegisterCaching();
        await HookPluginBuild();

        await BuildWebApplication();

        await PrepareDatabase();

        await UseBase();
        await UseOAuth2();
        await UseBaseMiddleware();
        await HookPluginConfigure();

        await MapBase();
        await MapOAuth2();
        await HookPluginEndpoints();

        await WebApplication.RunAsync();
    }

    private Task PrintVersion()
    {
        // Fancy start console output... yes very fancy :>
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

    private Task CreateStorage()
    {
        Directory.CreateDirectory("storage");
        Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));
        Directory.CreateDirectory(PathBuilder.Dir("storage", "plugins"));

        return Task.CompletedTask;
    }

    #region Base

    private Task RegisterBase()
    {
        WebApplicationBuilder.Services.AutoAddServices<Startup>();
        WebApplicationBuilder.Services.AddHttpClient();
        
        WebApplicationBuilder.Services.AddApiExceptionHandler();

        // Add pre-existing services
        WebApplicationBuilder.Services.AddSingleton(Configuration);
        WebApplicationBuilder.Services.AddSingleton(PluginService);

        // Configure controllers
        var mvcBuilder = WebApplicationBuilder.Services.AddControllers();

        // Add plugin and additional assemblies as application parts
        foreach (var pluginAssembly in PluginLoaderService.PluginAssemblies)
            mvcBuilder.AddApplicationPart(pluginAssembly);

        foreach (var additionalAssembly in AdditionalAssemblies)
            mvcBuilder.AddApplicationPart(additionalAssembly);

        return Task.CompletedTask;
    }

    private Task UseBase()
    {
        WebApplication.UseRouting();
        WebApplication.UseApiExceptionHandler();

        if (Configuration.Client.Enable)
        {
            if (WebApplication.Environment.IsDevelopment())
                WebApplication.UseWebAssemblyDebugging();

            WebApplication.UseBlazorFrameworkFiles();
            WebApplication.UseStaticFiles();
        }

        return Task.CompletedTask;
    }

    private Task UseBaseMiddleware()
    {
        WebApplication.UseMiddleware<AuthorizationMiddleware>();
        WebApplication.UseMiddleware<ApiAuthenticationMiddleware>();

        return Task.CompletedTask;
    }

    private Task MapBase()
    {
        WebApplication.MapControllers();

        if (Configuration.Client.Enable)
        {
            WebApplication.MapFallbackToFile("index.html");
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Interfaces

    private Task RegisterInterfaces()
    {
        WebApplicationBuilder.Services.AddInterfaces(configuration =>
        {
            // We use moonlight itself as a plugin assembly
            configuration.AddAssembly(typeof(Startup).Assembly);

            configuration.AddAssemblies(AdditionalAssemblies);
            configuration.AddAssemblies(PluginLoaderService.PluginAssemblies);

            configuration.AddInterface<IOAuth2Provider>();
            configuration.AddInterface<IAuthInterceptor>();
        });

        return Task.CompletedTask;
    }

    #endregion

    #region Plugin Loading

    private async Task LoadPlugins()
    {
        // Load plugins
        PluginService = new PluginService(
            LoggerFactory.CreateLogger<PluginService>()
        );

        await PluginService.Load();

        // Initialize api server plugin loader
        PluginLoaderService = new PluginLoaderService(
            LoggerFactory.CreateLogger<PluginLoaderService>()
        );

        // Search up entrypoints and assemblies for the apiServer
        var assemblyFiles = PluginService.GetAssemblies("apiServer")
            .Values
            .ToArray();

        var entrypoints = PluginService.GetEntrypoints("apiServer");

        // Build source from the retrieved data
        PluginLoaderService.AddFilesSource(assemblyFiles, entrypoints);

        // Perform assembly loading
        await PluginLoaderService.Load();
    }

    private Task InitializePlugins()
    {
        var initialisationServiceCollection = new ServiceCollection();

        // Configure base services for initialisation
        initialisationServiceCollection.AddSingleton(Configuration);

        initialisationServiceCollection.AddLogging(builder => { builder.AddProviders(LoggerProviders); });

        // Configure plugin loading by using the interface service
        initialisationServiceCollection.AddInterfaces(configuration =>
        {
            // We use moonlight itself as a plugin assembly
            configuration.AddAssembly(typeof(Startup).Assembly);

            configuration.AddAssemblies(PluginLoaderService.PluginAssemblies);
            configuration.AddAssemblies(AdditionalAssemblies);

            configuration.AddInterface<IAppStartup>();
            configuration.AddInterface<IDatabaseStartup>();
            configuration.AddInterface<IEndpointStartup>();
        });

        var initialisationServiceProvider = initialisationServiceCollection.BuildServiceProvider();

        PluginAppStartups = initialisationServiceProvider.GetRequiredService<IAppStartup[]>();
        PluginDatabaseStartups = initialisationServiceProvider.GetRequiredService<IDatabaseStartup[]>();
        PluginEndpointStartups = initialisationServiceProvider.GetRequiredService<IEndpointStartup[]>();

        return Task.CompletedTask;
    }

    #region Hooks

    private async Task HookPluginBuild()
    {
        foreach (var pluginAppStartup in PluginAppStartups)
        {
            try
            {
                await pluginAppStartup.BuildApp(WebApplicationBuilder);
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
                await pluginAppStartup.ConfigureApp(WebApplication);
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
        foreach (var pluginEndpointStartup in PluginEndpointStartups)
        {
            try
            {
                await pluginEndpointStartup.ConfigureEndpoints(WebApplication);
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

    #endregion

    #endregion

    #region Configurations

    private Task SetupAppConfiguration()
    {
        ConfigurationService = new ConfigurationService();

        // Setup options
        ConfigurationOptions = new ConfigurationOptions();

        ConfigurationOptions.AddConfiguration<AppConfiguration>("app");
        ConfigurationOptions.Path = PathBuilder.Dir("storage");
        ConfigurationOptions.EnvironmentPrefix = "MOONLIGHT";

        // Create minimal logger
        var loggerFactory = new LoggerFactory();

        loggerFactory.AddMoonCore(configuration =>
        {
            configuration.Console.Enable = true;
            configuration.Console.EnableAnsiMode = true;
            configuration.FileLogging.Enable = false;
        });

        var logger = loggerFactory.CreateLogger<ConfigurationService>();

        // Retrieve configuration
        Configuration = ConfigurationService.GetConfiguration<AppConfiguration>(
            ConfigurationOptions,
            logger
        );

        return Task.CompletedTask;
    }

    private Task RegisterAppConfiguration()
    {
        ConfigurationService.RegisterInDi(ConfigurationOptions, WebApplicationBuilder.Services);
        WebApplicationBuilder.Services.AddSingleton(ConfigurationService);

        return Task.CompletedTask;
    }

    #endregion

    #region Web Application

    private Task CreateWebApplicationBuilder()
    {
        WebApplicationBuilder = WebApplication.CreateBuilder(Args);
        return Task.CompletedTask;
    }

    private Task BuildWebApplication()
    {
        WebApplication = WebApplicationBuilder.Build();
        return Task.CompletedTask;
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

    private async Task RegisterLogging()
    {
        // Configure application logging
        WebApplicationBuilder.Logging.ClearProviders();
        WebApplicationBuilder.Logging.AddProviders(LoggerProviders);

        // Logging levels
        var logConfigPath = PathBuilder.File("storage", "logConfig.json");

        // Ensure logging config, add a default one is missing
        if (!File.Exists(logConfigPath))
        {
            var logLevels = new Dictionary<string, string>
            {
                { "Default", "Information" },
                { "Microsoft.AspNetCore", "Warning" },
                { "System.Net.Http.HttpClient", "Warning" }
            };

            var logLevelsJson = JsonSerializer.Serialize(logLevels);
            var logConfig = "{\"LogLevel\":" + logLevelsJson + "}";
            await File.WriteAllTextAsync(logConfigPath, logConfig);
        }

        // Add logging configuration
        WebApplicationBuilder.Logging.AddConfiguration(
            await File.ReadAllTextAsync(logConfigPath)
        );

        // Mute exception handler middleware
        // https://github.com/dotnet/aspnetcore/issues/19740
        WebApplicationBuilder.Logging.AddFilter(
            "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware",
            LogLevel.Critical
        );
        
        WebApplicationBuilder.Logging.AddFilter(
            "Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware",
            LogLevel.Critical
        );
    }

    #endregion

    #region Database

    private async Task RegisterDatabase()
    {
        var logger = LoggerFactory.CreateLogger<DatabaseHelper>();
        var databaseHelper = new DatabaseHelper(logger);

        var databaseCollection = new DatabaseContextCollection();

        foreach (var databaseStartup in PluginDatabaseStartups)
            await databaseStartup.ConfigureDatabase(databaseCollection);

        foreach (var database in databaseCollection)
        {
            databaseHelper.AddDbContext(database);
            WebApplicationBuilder.Services.AddScoped(database);
        }

        databaseHelper.GenerateMappings();

        WebApplicationBuilder.Services.AddSingleton(databaseHelper);
        WebApplicationBuilder.Services.AddScoped(typeof(DatabaseRepository<>));
        WebApplicationBuilder.Services.AddScoped(typeof(CrudHelper<,>));
    }

    private async Task PrepareDatabase()
    {
        using var scope = WebApplication.Services.CreateScope();
        var databaseHelper = scope.ServiceProvider.GetRequiredService<DatabaseHelper>();

        await databaseHelper.EnsureMigrated(scope.ServiceProvider);
    }

    #endregion

    #region OAuth2

    private Task RegisterOAuth2()
    {
        WebApplicationBuilder.Services.AddOAuth2Authentication<User>(configuration =>
        {
            configuration.AccessSecret = Configuration.Authentication.AccessSecret;
            configuration.RefreshSecret = Configuration.Authentication.RefreshSecret;
            configuration.RefreshDuration = TimeSpan.FromSeconds(Configuration.Authentication.RefreshDuration);
            configuration.RefreshInterval = TimeSpan.FromSeconds(Configuration.Authentication.AccessDuration);
            configuration.ClientId = Configuration.Authentication.OAuth2.ClientId;
            configuration.ClientSecret = Configuration.Authentication.OAuth2.ClientSecret;
            configuration.AuthorizeEndpoint = Configuration.PublicUrl + "/api/_auth/oauth2/authorize";
            configuration.RedirectUri = Configuration.PublicUrl;
        });

        WebApplicationBuilder.Services.AddScoped<IDataProvider<User>, LocalOAuth2Provider>();

        if (!Configuration.Authentication.UseLocalOAuth2)
            return Task.CompletedTask;

        WebApplicationBuilder.Services.AddLocalOAuth2Provider<User>(Configuration.PublicUrl);
        WebApplicationBuilder.Services.AddScoped<ILocalProviderImplementation<User>, LocalOAuth2Provider>();
        WebApplicationBuilder.Services.AddScoped<IOAuth2Provider<User>, LocalOAuth2Provider<User>>();

        return Task.CompletedTask;
    }

    private Task UseOAuth2()
    {
        WebApplication.UseOAuth2Authentication<User>();
        WebApplication.UseMiddleware<PermissionLoaderMiddleware>();

        return Task.CompletedTask;
    }

    private Task MapOAuth2()
    {
        WebApplication.MapOAuth2Authentication<User>();

        if (!Configuration.Authentication.UseLocalOAuth2)
            return Task.CompletedTask;

        WebApplication.MapLocalOAuth2Provider<User>();
        return Task.CompletedTask;
    }

    #endregion

    #region Caching

    private Task RegisterCaching()
    {
        WebApplicationBuilder.Services.AddMemoryCache();
        return Task.CompletedTask;
    }

    #endregion
}