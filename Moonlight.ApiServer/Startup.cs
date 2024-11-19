using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using MoonCore.Authentication;
using MoonCore.Exceptions;
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
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Http.Middleware;
using Moonlight.ApiServer.Implementations.OAuth2;
using Moonlight.ApiServer.Interfaces.Auth;
using Moonlight.ApiServer.Interfaces.OAuth2;
using Moonlight.ApiServer.Interfaces.Startup;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Responses.OAuth2;

namespace Moonlight.ApiServer;

public static class Startup
{
    public static async Task Main(string[] args)
        => await Run(args, []);

    public static async Task Run(string[] args, Assembly[]? additionalAssemblies = null)
    {
        // Cry about it
#pragma warning disable ASP0000

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

        // Storage i guess
        Directory.CreateDirectory(PathBuilder.Dir("storage"));

        // Configure startup logger
        var startupLoggerFactory = new LoggerFactory();

        // TODO: Add direct extension method
        var providers = LoggerBuildHelper.BuildFromConfiguration(configuration =>
        {
            configuration.Console.Enable = true;
            configuration.Console.EnableAnsiMode = true;
            configuration.FileLogging.Enable = false;
        });

        startupLoggerFactory.AddProviders(providers);

        var startupLogger = startupLoggerFactory.CreateLogger("Startup");

        // Load plugins
        var pluginService = new PluginService(
            startupLoggerFactory.CreateLogger<PluginService>()
        );

        await pluginService.Load();

        var pluginAssemblies = await LoadPlugins(pluginService, startupLoggerFactory);

        // Configure startup interfaces
        var startupServiceCollection = new ServiceCollection();

        startupServiceCollection.AddConfiguration(options =>
        {
            options.UsePath(PathBuilder.Dir("storage"));
            options.UseEnvironmentPrefix("MOONLIGHT");

            options.AddConfiguration<AppConfiguration>("app");
        });

        startupServiceCollection.AddLogging(loggingBuilder => { loggingBuilder.AddProviders(providers); });

        startupServiceCollection.AddPlugins(configuration =>
        {
            // Configure startup interfaces
            configuration.AddInterface<IAppStartup>();
            configuration.AddInterface<IDatabaseStartup>();
            configuration.AddInterface<IEndpointStartup>();

            // Configure assemblies to scan
            configuration.AddAssembly(typeof(Startup).Assembly);

            if (additionalAssemblies != null)
                configuration.AddAssemblies(additionalAssemblies);

            configuration.AddAssemblies(pluginAssemblies);
        });


        var startupServiceProvider = startupServiceCollection.BuildServiceProvider();
        var appStartupInterfaces = startupServiceProvider.GetRequiredService<IAppStartup[]>();

        var config = startupServiceProvider.GetRequiredService<AppConfiguration>();
        ApplicationStateHelper.SetConfiguration(config);

        // Start the actual app
        var builder = WebApplication.CreateBuilder(args);

        await ConfigureLogging(builder);

        await ConfigureDatabase(
            builder,
            startupLoggerFactory,
            startupServiceProvider.GetRequiredService<IDatabaseStartup[]>()
        );

        // Call interfaces
        foreach (var startupInterface in appStartupInterfaces)
        {
            try
            {
                await startupInterface.BuildApp(builder);
            }
            catch (Exception e)
            {
                startupLogger.LogCritical(
                    "An unhandled error occured while processing BuildApp call for interface '{interfaceName}': {e}",
                    startupInterface.GetType().FullName,
                    e
                );
            }
        }

        var controllerBuilder = builder.Services.AddControllers();

        // Add current assemblies to the application part
        foreach (var moduleAssembly in pluginAssemblies)
            controllerBuilder.AddApplicationPart(moduleAssembly);

        builder.Services.AddSingleton(config);
        builder.Services.AddSingleton(pluginService);
        builder.Services.AutoAddServices(typeof(Startup).Assembly);
        builder.Services.AddHttpClient();

        await ConfigureCaching(builder, startupLogger, config);

        await ConfigureOAuth2(builder, startupLogger, config);

        // Implementation service
        builder.Services.AddPlugins(configuration =>
        {
            configuration.AddInterface<IOAuth2Provider>();
            configuration.AddInterface<IAuthInterceptor>();

            configuration.AddAssembly(typeof(Startup).Assembly);

            if (additionalAssemblies != null)
                configuration.AddAssemblies(additionalAssemblies);

            configuration.AddAssemblies(pluginAssemblies);
        });

        var app = builder.Build();

        await PrepareDatabase(app);

        if (config.Client.Enable)
        {
            if (app.Environment.IsDevelopment())
                app.UseWebAssemblyDebugging();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
        }

        app.UseRouting();

        app.UseApiErrorHandling();

        await UseOAuth2(app);

        // Call interfaces
        foreach (var startupInterface in appStartupInterfaces)
        {
            try
            {
                await startupInterface.ConfigureApp(app);
            }
            catch (Exception e)
            {
                startupLogger.LogCritical(
                    "An unhandled error occured while processing ConfigureApp call for interface '{interfaceName}': {e}",
                    startupInterface.GetType().FullName,
                    e
                );
            }
        }

        app.UseMiddleware<ApiAuthenticationMiddleware>();

        app.UseMiddleware<AuthorizationMiddleware>();

        // Call interfaces
        var endpointStartupInterfaces = startupServiceProvider.GetRequiredService<IEndpointStartup[]>();

        foreach (var endpointStartup in endpointStartupInterfaces)
        {
            try
            {
                await endpointStartup.ConfigureEndpoints(app);
            }
            catch (Exception e)
            {
                startupLogger.LogCritical(
                    "An unhandled error occured while processing ConfigureEndpoints call for interface '{interfaceName}': {e}",
                    endpointStartup.GetType().FullName,
                    e
                );
            }
        }

        app.MapControllers();

        if (config.Client.Enable)
            app.MapFallbackToFile("index.html");

        await app.RunAsync();
    }

    #region Logging

    public static async Task ConfigureLogging(IHostApplicationBuilder builder)
    {
        // Create logging path
        Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));

        // Configure application logging
        builder.Logging.ClearProviders();

        builder.Logging.AddMoonCore(configuration =>
        {
            configuration.Console.Enable = true;
            configuration.Console.EnableAnsiMode = true;

            configuration.FileLogging.Enable = true;
            configuration.FileLogging.Path = PathBuilder.File("storage", "logs", "moonlight.log");
            configuration.FileLogging.EnableLogRotation = true;
            configuration.FileLogging.RotateLogNameTemplate = PathBuilder.File("storage", "logs", "moonlight.log.{0}");
        });

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

        builder.Logging.AddConfiguration(await File.ReadAllTextAsync(logConfigPath));
    }

    #endregion

    #region Database

    public static async Task ConfigureDatabase(IHostApplicationBuilder builder, ILoggerFactory loggerFactory,
        IDatabaseStartup[] databaseStartups)
    {
        var logger = loggerFactory.CreateLogger<DatabaseHelper>();
        var databaseHelper = new DatabaseHelper(logger);

        var databaseCollection = new DatabaseContextCollection();

        foreach (var databaseStartup in databaseStartups)
            await databaseStartup.ConfigureDatabase(databaseCollection);

        foreach (var database in databaseCollection)
        {
            databaseHelper.AddDbContext(database);
            builder.Services.AddScoped(database);
        }

        databaseHelper.GenerateMappings();

        builder.Services.AddSingleton(databaseHelper);
        builder.Services.AddScoped(typeof(DatabaseRepository<>));
        builder.Services.AddScoped(typeof(CrudHelper<,>));
    }

    public static async Task PrepareDatabase(IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();
        var databaseHelper = scope.ServiceProvider.GetRequiredService<DatabaseHelper>();

        await databaseHelper.EnsureMigrated(scope.ServiceProvider);
    }

    #endregion

    #region OAuth2

    public static Task ConfigureOAuth2(WebApplicationBuilder builder, ILogger logger, AppConfiguration config)
    {
        builder.AddOAuth2Authentication<User>(configuration =>
        {
            configuration.AccessSecret = config.Authentication.AccessSecret;
            configuration.RefreshSecret = config.Authentication.RefreshSecret;
            configuration.RefreshDuration = TimeSpan.FromSeconds(config.Authentication.RefreshDuration);
            configuration.RefreshInterval = TimeSpan.FromSeconds(config.Authentication.AccessDuration);
            configuration.ClientId = config.Authentication.OAuth2.ClientId;
            configuration.ClientSecret = config.Authentication.OAuth2.ClientSecret;
            configuration.AuthorizeEndpoint = config.PublicUrl + "/api/_auth/oauth2/authorize";
            configuration.RedirectUri = config.PublicUrl;
        });

        builder.Services.AddScoped<IDataProvider<User>, LocalOAuth2Provider>();

        if (config.Authentication.UseLocalOAuth2)
        {
            builder.AddLocalOAuth2Provider<User>(config.PublicUrl);
            builder.Services.AddScoped<ILocalProviderImplementation<User>, LocalOAuth2Provider>();
            builder.Services.AddScoped<IOAuth2Provider<User>, LocalOAuth2Provider<User>>();
        }

        return Task.CompletedTask;
    }

    public static Task UseOAuth2(WebApplication application)
    {
        application.UseOAuth2Authentication<User>();
        application.UseLocalOAuth2Provider<User>();

        application.UseMiddleware<PermissionLoaderMiddleware>();

        return Task.CompletedTask;
    }

    #endregion

    #region Caching

    public static Task ConfigureCaching(WebApplicationBuilder builder, ILogger logger, AppConfiguration configuration)
    {
        builder.Services.AddMemoryCache();
        return Task.CompletedTask;
    }

    #endregion

    #region Plugin loading

    private static async Task<Assembly[]> LoadPlugins(PluginService pluginService, ILoggerFactory loggerFactory)
    {
        var pluginLoader = new PluginLoaderService(
            loggerFactory.CreateLogger<PluginLoaderService>()
        );

        var assemblyFiles = pluginService.GetAssemblies("apiServer").Values.ToArray();
        var entrypoints = pluginService.GetEntrypoints("apiServer");

        pluginLoader.AddFilesSource(assemblyFiles, entrypoints);

        await pluginLoader.Load();

        return pluginLoader.PluginAssemblies;
    }

    #endregion
}