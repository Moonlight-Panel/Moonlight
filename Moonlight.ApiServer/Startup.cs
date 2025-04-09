using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using Hangfire;
using Hangfire.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoonCore.Configuration;
using MoonCore.EnvConfiguration;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Extensions;
using MoonCore.Extended.Helpers;
using MoonCore.Extended.JwtInvalidation;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Interfaces.Startup;
using Moonlight.ApiServer.Models;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer;

// Cry about it
#pragma warning disable ASP0000

public class Startup
{
    private string[] Args;
    private Assembly[] AdditionalAssemblies;
    private PluginManifest[] AdditionalPluginManifests;

    // Logging
    private ILoggerProvider[] LoggerProviders;
    private ILoggerFactory LoggerFactory;
    private ILogger<Startup> Logger;

    // Configuration
    private AppConfiguration Configuration;
    private IConfigurationRoot ConfigurationRoot;

    // WebApplication Stuff
    private WebApplication WebApplication;
    private WebApplicationBuilder WebApplicationBuilder;

    // Plugin Loading
    private PluginService PluginService;
    private AssemblyLoadContext PluginLoadContext;

    // Asset bundling
    private BundleService BundleService = new();

    private IPluginStartup[] PluginStartups;

    public async Task Run(string[] args, Assembly[]? additionalAssemblies = null,
        PluginManifest[]? additionalManifests = null)
    {
        Args = args;
        AdditionalAssemblies = additionalAssemblies ?? [];
        AdditionalPluginManifests = additionalManifests ?? [];

        await PrintVersion();

        await CreateStorage();
        await SetupAppConfiguration();
        await SetupLogging();
        await SetupBundling();
        await LoadPlugins();
        await InitializePlugins();

        await CreateWebApplicationBuilder();

        await ConfigureKestrel();
        await RegisterAppConfiguration();
        await RegisterLogging();
        await RegisterBase();
        await RegisterDatabase();
        await RegisterAuth();
        await RegisterCors();
        await RegisterHangfire();
        await HookPluginBuild();
        await RegisterPluginAssets();

        await BuildWebApplication();

        await PrepareDatabase();

        await UseCors();
        await UsePluginAssets(); // We need to move the plugin assets to the top to allow plugins to override content
        await UseBase();
        await UseAuth();
        await UseHangfire();
        await HookPluginConfigure();

        await MapBase();
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
        Directory.CreateDirectory(PathBuilder.Dir("storage", "plugins"));

        return Task.CompletedTask;
    }

    private Task SetupBundling()
    {
        BundleService = new();

        BundleService.BundleCss("css/core.min.css");

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
        foreach (var pluginAssembly in PluginLoadContext.Assemblies)
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

    private Task MapBase()
    {
        WebApplication.MapControllers();

        if (Configuration.Client.Enable)
            WebApplication.MapFallbackToFile("index.html");

        return Task.CompletedTask;
    }

    private Task ConfigureKestrel()
    {
        WebApplicationBuilder.WebHost.ConfigureKestrel(kestrelOptions =>
        {
            var maxUploadInBytes = ByteConverter
                .FromMegaBytes(Configuration.Kestrel.UploadLimit)
                .Bytes;

            kestrelOptions.Limits.MaxRequestBodySize = maxUploadInBytes;
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

        // Add plugins manually if specified in the startup
        foreach (var manifest in AdditionalPluginManifests)
            PluginService.LoadedPlugins.Add(manifest, Directory.GetCurrentDirectory());

        // Search and load all plugins
        await PluginService.Load();

        // Search up assemblies for the apiServer
        var assemblyFiles = PluginService.GetAssemblies("apiServer")
            .Values
            .ToArray();

        // Create the load context and add assemblies
        PluginLoadContext = new AssemblyLoadContext(null);

        foreach (var assemblyFile in assemblyFiles)
        {
            try
            {
                PluginLoadContext.LoadFromAssemblyPath(assemblyFile);
            }
            catch (Exception e)
            {
                Logger.LogError("Unable to load plugin assembly '{assemblyFile}': {e}", assemblyFile, e);
            }
        }
    }

    private Task InitializePlugins()
    {
        // Define minimal service collection
        var startupSc = new ServiceCollection();

        // Configure base services for initialisation
        startupSc.AddSingleton(Configuration);

        // Add bundle service so plugins can do additional bundling if required
        startupSc.AddSingleton(BundleService);

        // Auto add all files specified in the bundledStyles section to the bundle job
        foreach (var plugin in PluginService.LoadedPlugins.Keys)
            BundleService.BundleCssRange(plugin.BundledStyles);

        startupSc.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProviders(LoggerProviders);
        });

        //
        var startupSp = startupSc.BuildServiceProvider();

        // Initialize plugin startups
        var startups = new List<IPluginStartup>();
        var startupType = typeof(IPluginStartup);

        var assembliesToScan = new List<Assembly>();

        assembliesToScan.Add(typeof(Startup).Assembly);
        assembliesToScan.AddRange(PluginLoadContext.Assemblies);
        assembliesToScan.AddRange(AdditionalAssemblies);

        foreach (var pluginAssembly in assembliesToScan)
        {
            var startupTypes = pluginAssembly
                .ExportedTypes
                .Where(x => !x.IsAbstract && !x.IsInterface && x.IsAssignableTo(startupType))
                .ToArray();

            foreach (var type in startupTypes)
            {
                var startup = ActivatorUtilities.CreateInstance(startupSp, type) as IPluginStartup;

                if (startup == null)
                    continue;

                startups.Add(startup);
            }
        }

        PluginStartups = startups.ToArray();

        return Task.CompletedTask;
    }

    private Task RegisterPluginAssets()
    {
        WebApplicationBuilder.Services.AddHostedService(sp => sp.GetRequiredService<BundleGenerationService>());
        WebApplicationBuilder.Services.AddSingleton<BundleGenerationService>();
        WebApplicationBuilder.Services.AddSingleton(BundleService);

        return Task.CompletedTask;
    }

    private Task UsePluginAssets()
    {
        WebApplication.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = new BundleAssetFileProvider()
        });

        WebApplication.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = PluginService.WwwRootFileProvider
        });

        return Task.CompletedTask;
    }

    #region Hooks

    private async Task HookPluginBuild()
    {
        foreach (var pluginAppStartup in PluginStartups)
        {
            try
            {
                await pluginAppStartup.BuildApplication(WebApplicationBuilder);
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
                await pluginAppStartup.ConfigureApplication(WebApplication);
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
        foreach (var pluginEndpointStartup in PluginStartups)
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

    private async Task SetupAppConfiguration()
    {
        // Configure configuration (wow)
        var configurationBuilder = new ConfigurationBuilder();

        // Ensure configuration file exists
        var jsonFilePath = PathBuilder.File(Directory.GetCurrentDirectory(), "storage", "app.json");

        if (!File.Exists(jsonFilePath))
            await File.WriteAllTextAsync(jsonFilePath, JsonSerializer.Serialize(new AppConfiguration()));

        configurationBuilder.AddJsonFile(
            jsonFilePath
        );

        configurationBuilder.AddEnvironmentVariables(prefix: "MOONLIGHT_", separator: "_");

        ConfigurationRoot = configurationBuilder.Build();

        // Retrieve configuration
        Configuration = ConfigurationRoot.Get<AppConfiguration>()!;
    }

    private Task RegisterAppConfiguration()
    {
        WebApplicationBuilder.Services.AddSingleton(Configuration);
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

    private Task RegisterDatabase()
    {
        WebApplicationBuilder.Services.AddDatabaseMappings();
        WebApplicationBuilder.Services.AddServiceCollectionAccessor();

        WebApplicationBuilder.Services.AddScoped(typeof(DatabaseRepository<>));
        WebApplicationBuilder.Services.AddScoped(typeof(CrudHelper<,>));

        return Task.CompletedTask;
    }

    private async Task PrepareDatabase()
    {
        await WebApplication.Services.EnsureDatabaseMigrated();

        WebApplication.Services.GenerateDatabaseMappings();
    }

    #endregion

    #region Authentication & Authorisation

    private Task RegisterAuth()
    {
        WebApplicationBuilder.Services
            .AddAuthentication("coreAuthentication")
            .AddJwtBearer("coreAuthentication", options =>
            {
                options.TokenValidationParameters = new()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        Configuration.Authentication.Secret
                    )),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateAudience = true,
                    ValidAudience = Configuration.PublicUrl,
                    ValidateIssuer = true,
                    ValidIssuer = Configuration.PublicUrl
                };
            });

        WebApplicationBuilder.Services.AddJwtInvalidation("coreAuthentication", options =>
        {
            options.InvalidateTimeProvider = async (provider, principal) =>
            {
                var userIdClaim = principal.Claims.FirstOrDefault(x => x.Type == "userId");

                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);

                    var userRepository = provider.GetRequiredService<DatabaseRepository<User>>();
                    var user = await userRepository.Get().FirstOrDefaultAsync(x => x.Id == userId);

                    if (user == null)
                        return DateTime.MaxValue;

                    return user.TokenValidTimestamp;
                }

                var apiKeyIdClaim = principal.Claims.FirstOrDefault(x => x.Type == "apiKeyId");

                if (apiKeyIdClaim != null)
                {
                    var apiKeyId = int.Parse(apiKeyIdClaim.Value);

                    var apiKeyRepository = provider.GetRequiredService<DatabaseRepository<ApiKey>>();
                    var apiKey = await apiKeyRepository.Get().FirstOrDefaultAsync(x => x.Id == apiKeyId);

                    // If the api key exists, we don't want to invalidate the request.
                    // If it doesn't exist we want to invalidate the request
                    return apiKey == null ? DateTime.MaxValue : DateTime.MinValue;
                }

                return DateTime.MaxValue;
            };
        });

        WebApplicationBuilder.Services.AddAuthorization();

        return Task.CompletedTask;
    }

    private Task UseAuth()
    {
        WebApplication.UseAuthentication();

        WebApplication.UseJwtInvalidation();

        WebApplication.UseAuthorization();

        return Task.CompletedTask;
    }

    #endregion

    #region Cors

    private Task RegisterCors()
    {
        WebApplicationBuilder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().Build();
            });
        });

        return Task.CompletedTask;
    }

    private Task UseCors()
    {
        WebApplication.UseCors();

        return Task.CompletedTask;
    }

    #endregion

    #region Hangfire

    private Task RegisterHangfire()
    {
        WebApplicationBuilder.Services.AddHangfire((provider, configuration) =>
        {
            configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
            configuration.UseSimpleAssemblyNameTypeSerializer();
            configuration.UseRecommendedSerializerSettings();
            configuration.UseEFCoreStorage(() =>
            {
                var scope = provider.CreateScope();
                return scope.ServiceProvider.GetRequiredService<CoreDataContext>();
            }, new EFCoreStorageOptions());
        });

        WebApplicationBuilder.Services.AddHangfireServer();

        WebApplicationBuilder.Logging.AddFilter(
            "Hangfire.Server.BackgroundServerProcess",
            LogLevel.Warning
        );
        
        WebApplicationBuilder.Logging.AddFilter(
            "Hangfire.BackgroundJobServer",
            LogLevel.Warning
        );

        return Task.CompletedTask;
    }

    private Task UseHangfire()
    {
        if (WebApplication.Environment.IsDevelopment())
            WebApplication.UseHangfireDashboard();
        
        return Task.CompletedTask;
    }

    #endregion
}