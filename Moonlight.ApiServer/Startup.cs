using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoonCore.Configuration;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Extensions;
using MoonCore.Extended.Helpers;
using MoonCore.Extended.JwtInvalidation;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.PluginFramework.Extensions;
using MoonCore.Plugins;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers;
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
    
    // Asset bundling
    private BundleService BundleService;

    private IPluginStartup[] PluginStartups;

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
        await RegisterAuth();
        await RegisterCaching();
        await HookPluginBuild();
        await HandleConfigureArguments();
        await RegisterPluginAssets();

        await BuildWebApplication();

        await HandleServiceArguments();
        await PrepareDatabase();

        await UseBase();
        await UseAuth();
        await HookPluginConfigure();
        await UsePluginAssets();

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
        Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));
        Directory.CreateDirectory(PathBuilder.Dir("storage", "plugins"));

        return Task.CompletedTask;
    }

    #region Command line arguments

    private Task HandleConfigureArguments()
    {
        return Task.CompletedTask;
    }
    
    private Task HandleServiceArguments()
    {
        // Handle manual asset loading arguments
        if (Args.Any(x => x.StartsWith("--frontend-asset")))
        {
            if (!Configuration.Client.Enable)
            {
                Logger.LogWarning("The hosting of the moonlight frontend is disabled. Ignoring all --frontend-asset options");
                return Task.CompletedTask; // TODO: Change this when adding more service argument handling functions
            }

            if (!WebApplicationBuilder.Environment.IsDevelopment())
                Logger.LogWarning("Using the --frontend-asset option is not meant to be used in production. Plugin assets will be loaded automaticly");
            
            var assetService = WebApplication.Services.GetRequiredService<AssetService>();
            
            for (var i = 0; i < Args.Length; i++)
            {
                var currentArg = Args[i];
                
                // Ignore all args without relation to our frontend assets
                if(!currentArg.Equals("--frontend-asset", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (i + 1 >= Args.Length)
                {
                    Logger.LogWarning("You need to specify an asset path after the --frontend-asset option");
                    continue;
                }

                var nextArg = Args[i + 1];

                if (nextArg.StartsWith("--"))
                {
                    Logger.LogWarning("You need to specify an asset path after the --frontend-asset option");
                    continue;
                }

                var extension = Path.GetExtension(nextArg);

                switch (extension)
                {
                    case ".css":
                        BundleService.BundleCss(nextArg);
                        break;
                    case ".js":
                        assetService.AddJavascriptAsset(nextArg);
                        break;
                    default:
                        Logger.LogWarning("Unknown asset extension {extension}. Ignoring it", extension);
                        break;
                }
            }
        }
        
        return Task.CompletedTask;
    }

    #endregion

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
        // Define minimal service collection
        var startupSc = new ServiceCollection();

        // Configure base services for initialisation
        startupSc.AddSingleton(Configuration);
        
        BundleService = new BundleService();
        startupSc.AddSingleton(BundleService);
            
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
        assembliesToScan.AddRange(PluginLoaderService.PluginAssemblies);
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
                
                if(startup == null)
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
            FileProvider = new PluginAssetFileProvider(PluginService)
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
        WebApplicationBuilder.Services.AddDatabaseMappings();
        
        WebApplicationBuilder.Services.AddScoped(typeof(DatabaseRepository<>));
        WebApplicationBuilder.Services.AddScoped(typeof(CrudHelper<,>));
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
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
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
        
        WebApplicationBuilder.Services.AddJwtInvalidation(options =>
        {
            options.InvalidateTimeProvider = async (provider, principal) =>
            {
                var userIdClaim = principal.Claims.First(x => x.Type == "userId");
                var userId = int.Parse(userIdClaim.Value);
                
                var userRepository = provider.GetRequiredService<DatabaseRepository<User>>();
                var user = await userRepository.Get().FirstOrDefaultAsync(x => x.Id == userId);
                
                if(user == null)
                    return DateTime.MaxValue;

                return user.TokenValidTimestamp;
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

    #region Caching

    private Task RegisterCaching()
    {
        WebApplicationBuilder.Services.AddMemoryCache();
        return Task.CompletedTask;
    }

    #endregion
}