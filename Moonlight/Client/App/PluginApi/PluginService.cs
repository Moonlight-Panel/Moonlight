using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using MoonCore.Helpers;

namespace Moonlight.Client.App.PluginApi;

public class PluginService
{
    private readonly ILogger<PluginService> Logger;
    private readonly ILoggerFactory LoggerFactory;
    
    private readonly Dictionary<Type, List<object>> InterfaceImplementations = new();
    
    public readonly List<Assembly> PluginAssemblies = new();
    public readonly List<Assembly> LibraryAssemblies = new();
    public readonly List<MoonlightClientPlugin> LoadedPlugins = new();

    public PluginService(ILogger<PluginService> logger, ILoggerFactory loggerFactory)
    {
        Logger = logger;
        LoggerFactory = loggerFactory;
    }

    public async Task Load(string baseUrl)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(baseUrl);

        var dllFiles = Array.Empty<string>();

        try
        {
            var jsonText = await httpClient.GetStringAsync("clientPlugins");
            dllFiles = JsonSerializer.Deserialize<string[]>(jsonText) ?? [];
        }
        catch (Exception e)
        {
            Logger.LogError("An error occured while fetching plugins from server: {e}", e);
        }

        if (dllFiles.Length == 0)
        {
            Logger.LogInformation("No plugins found in the plugins manifest");
            return;
        }

        var pluginTypes = new List<Type>();
        var pluginType = typeof(MoonlightClientPlugin);

        var context = new AssemblyLoadContext(null);
        
        // Load all plugins into one context so the plugins are able to load their dependencies
        foreach (var dllFile in dllFiles)
        {
            try
            {
                //TODO: Cache plugins in cache
                var assemblyStream = await httpClient.GetStreamAsync($"clientPlugins/stream?name={dllFile}");
                context.LoadFromStream(assemblyStream);
            }
            catch (Exception e)
            {
                Logger.LogError("Unable to load the dll file '{file}' because an error occured: {e}", dllFile, e);
            }
        }

        // After all dll files are loaded, we can finally check the assemblies for plugins.
        // If we did it while loading, we would get resolve errors when a plugin dll requires another
        // dll as a library
        foreach (var assembly in context.Assemblies)
        {
            var plugins = assembly.ExportedTypes
                .Where(x => x.IsSubclassOf(pluginType))
                .ToArray();

            if (plugins.Length == 0)
            {
                Logger.LogInformation("Loaded '{file}' as library", assembly.FullName);
                LibraryAssemblies.Add(assembly);
            }
            else
            {
                pluginTypes.AddRange(plugins);
                PluginAssemblies.Add(assembly);
            }
        }

        Logger.LogInformation("Initializing plugins");
        foreach (var type in pluginTypes)
        {
            try
            {
                var pl = Activator.CreateInstance(type, [
                        LoggerFactory.CreateLogger(type),
                        this
                    ]
                ) as MoonlightClientPlugin;

                LoadedPlugins.Add(pl!);

                Logger.LogInformation("Instantiated plugin '{name}'", type.FullName);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while instantiating the plugin '{name}': {e}", type.FullName, e);
            }
        }

        Logger.LogInformation("Loading plugins");
        foreach (var plugin in LoadedPlugins)
            await plugin.OnLoaded();
    }

    public async Task CallPlugins(Func<MoonlightClientPlugin, Task> func)
    {
        foreach (var plugin in LoadedPlugins)
            await func.Invoke(plugin);
    }

    public void RegisterImplementation<T>(T implementation) where T : class
    {
        var interfaceType = typeof(T);

        lock (InterfaceImplementations)
        {
            if(!InterfaceImplementations.ContainsKey(interfaceType))
                InterfaceImplementations.Add(interfaceType, new());

            InterfaceImplementations[interfaceType].Add(implementation);
        }
    }

    public void RegisterImplementation<T, TImpl>() where TImpl : T where T : class
    {
        var impl = Activator.CreateInstance<TImpl>();

        RegisterImplementation<T>(impl);
    }

    public T[] GetImplementations<T>() where T : class
    {
        var interfaceType = typeof(T);

        lock (InterfaceImplementations)
        {
            if (!InterfaceImplementations.ContainsKey(interfaceType))
                return [];

            return InterfaceImplementations[interfaceType]
                .Select(x => (T)x)
                .ToArray();
        }
    }

    public async Task ExecuteImplementations<T>(Func<T, Task> func) where T : class
    {
        var implementations = GetImplementations<T>();

        foreach (var implementation in implementations)
            await func.Invoke(implementation);
    }
}