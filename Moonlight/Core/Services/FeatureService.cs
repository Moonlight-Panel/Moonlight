using System.Reflection;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Services;
using MoonCoreUI.Components;
using Moonlight.Core.Configuration;
using Moonlight.Core.Models.Abstractions.Feature;

namespace Moonlight.Core.Services;

public class FeatureService
{
    public readonly UiInitContext UiContext = new();
    public readonly PreInitContext PreInitContext = new();
    
    private readonly List<MoonlightFeature> Features = new();
    private readonly ConfigService<CoreConfiguration> ConfigService;
    
    public FeatureService(ConfigService<CoreConfiguration> configService)
    {
        ConfigService = configService;
    }

    public Task Load()
    {
        Logger.Info("Loading features");
        
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
                Logger.Warn($"Unable to construct {featureType.FullName} feature");
                continue;
            }
            
            Features.Add(feature);
            
            Logger.Info($"Loaded feature '{feature.Name}' by '{feature.Author}'");
        }
        
        return Task.CompletedTask;
    }

    public async Task PreInit(WebApplicationBuilder builder)
    {
        Logger.Info("Pre-initializing features");

        PreInitContext.Builder = builder;

        foreach (var feature in Features)
        {
            try
            {
                await feature.OnPreInitialized(PreInitContext);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occured while performing pre init for feature '{feature.Name}'");
                Logger.Error(e);
            }
        }

        // Process every di assembly with moon core
        foreach (var assembly in PreInitContext.DiAssemblies)
            builder.Services.ConstructMoonCoreDi(assembly);
    }
    
    public async Task Init(WebApplication application)
    {
        Logger.Info("Initializing features");

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
                Logger.Error($"An error occured while performing init for feature '{feature.Name}'");
                Logger.Error(e);
            }
        }
    }

    public async Task UiInit()
    {
        Logger.Info("Initializing feature uis");

        foreach (var feature in Features)
        {
            try
            {
                await feature.OnUiInitialized(UiContext);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occured while performing ui init for feature '{feature.Name}'");
                Logger.Error(e);
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
                Logger.Error($"An error occured while performing session init for feature '{feature.Name}'");
                Logger.Error(e);
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
                Logger.Error($"An error occured while performing session dispose for feature '{feature.Name}'");
                Logger.Error(e);
            }
        }
    }

    public Task<MoonlightFeature[]> GetLoadedFeatures()
    {
        return Task.FromResult(Features.ToArray());
    }
}