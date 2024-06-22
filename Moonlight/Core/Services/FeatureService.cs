using System.Reflection;
using MoonCore.Blazor.Components;
using MoonCore.Extensions;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Models.Abstractions.Feature;

namespace Moonlight.Core.Services;

public class FeatureService
{
    public readonly UiInitContext UiContext = new();
    public readonly PreInitContext PreInitContext = new();
    
    private readonly List<MoonlightFeature> Features = new();
    private readonly ConfigService<CoreConfiguration> ConfigService;

    private readonly ILogger<FeatureService> Logger;
    
    public FeatureService(ConfigService<CoreConfiguration> configService, ILogger<FeatureService> logger)
    {
        ConfigService = configService;
        Logger = logger;
    }

    public Task Load()
    {
        Logger.LogInformation("Loading features");
        
        // TODO: Add dll loading here as well

        var config = ConfigService.Get().Features;

        // This loads all features from the current assembly which have not been disabled in the config
        var featureTypes = Assembly
            .GetCallingAssembly()
            .GetTypes()
            .Where(x => x.IsSubclassOf(typeof(MoonlightFeature)))
            .Where(x => !config.DisableFeatures.Contains(x.FullName ?? "N/A"))
            .ToArray();
        
        foreach (var featureType in featureTypes)
        {
            var feature = Activator.CreateInstance(featureType) as MoonlightFeature;

            if (feature == null)
            {
                Logger.LogWarning("Unable to construct '{name}' feature", featureType.FullName);
                continue;
            }
            
            Features.Add(feature);
            
            Logger.LogInformation("Loaded feature '{name}' by '{author}'", feature.Name, feature.Author);
        }
        
        return Task.CompletedTask;
    }

    public async Task PreInit(WebApplicationBuilder builder, PluginService pluginService, ILoggerFactory preRunLoggerFactory)
    {
        Logger.LogInformation("Pre-initializing features");

        PreInitContext.Builder = builder;
        PreInitContext.Plugins = pluginService;
        PreInitContext.LoggerFactory = preRunLoggerFactory;

        foreach (var feature in Features)
        {
            try
            {
                await feature.OnPreInitialized(PreInitContext);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while performing pre init for feature '{name}': {e}", feature.Name, e);
            }
        }

        // Process every di assembly with moon core
        foreach (var assembly in PreInitContext.DiAssemblies)
            builder.Services.ConstructMoonCoreDi(assembly);
    }
    
    public async Task Init(WebApplication application)
    {
        Logger.LogInformation("Initializing features");

        var initContext = new InitContext()
        {
            Application = application
        };

        foreach (var feature in Features)
        {
            try
            {
                await feature.OnInitialized(initContext);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while performing init for feature '{name}': {e}", feature.Name, e);
            }
        }
    }

    public async Task UiInit()
    {
        Logger.LogInformation("Initializing feature uis");

        foreach (var feature in Features)
        {
            try
            {
                await feature.OnUiInitialized(UiContext);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while performing ui init for feature '{name}': {e}", feature.Name, e);
            }
        }
    }

    public async Task SessionInit(IServiceProvider provider, LazyLoader lazyLoader)
    {
        var context = new SessionInitContext()
        {
            ServiceProvider = provider,
            LazyLoader = lazyLoader
        };
        
        foreach (var feature in Features)
        {
            try
            {
                await lazyLoader.SetText($"Initializing {feature.Name}");
                await feature.OnSessionInitialized(context);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while performing session init for feature '{name}': {e}", feature.Name, e);
            }
        }
    }
    
    public async Task SessionDispose(ScopedStorageService scopedStorageService)
    {
        var context = new SessionDisposeContext()
        {
            ScopedStorageService = scopedStorageService
        };
        
        foreach (var feature in Features)
        {
            try
            {
                await feature.OnSessionDisposed(context);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while performing session dispose for feature '{name}': {e}", feature.Name, e);
            }
        }
    }

    public Task<MoonlightFeature[]> GetLoadedFeatures()
    {
        return Task.FromResult(Features.ToArray());
    }
}