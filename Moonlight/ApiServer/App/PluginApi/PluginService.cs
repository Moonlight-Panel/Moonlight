using System.Reflection;
using System.Runtime.Loader;
using MoonCore.Helpers;

namespace Moonlight.ApiServer.App.PluginApi;

public class PluginService
{
    private readonly ILogger<PluginService> Logger;
    private readonly ILoggerFactory LoggerFactory;
    
    private readonly Dictionary<Type, List<object>> InterfaceImplementations = new();

    public readonly List<Assembly> PluginAssemblies = new();
    public readonly List<Assembly> LibraryAssemblies = new();
    public readonly List<MoonlightPlugin> LoadedPlugins = new();

    public PluginService(ILogger<PluginService> logger, ILoggerFactory loggerFactory)
    {
        Logger = logger;
        LoggerFactory = loggerFactory;
    }

    public async Task Load()
    {
        var dllFilesInFolder = new List<string>();
        GetDllFiles(PathBuilder.Dir("storage", "plugins"), dllFilesInFolder);

        if (dllFilesInFolder.Count == 0)
        {
            Logger.LogInformation("No plugins found in the plugins folder");
            return;
        }

        var pluginTypes = new List<Type>();
        var pluginType = typeof(MoonlightPlugin);
        
        var context = new AssemblyLoadContext(null);

        // Load all plugins into one context so the plugins are able to load their dependencies
        foreach (var dllFile in dllFilesInFolder)
        {
            try
            {
                var fs = File.OpenRead(dllFile);
                context.LoadFromStream(fs);
                fs.Close();
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
                Logger.LogInformation("Loaded '{file}' as library", assembly.Location);
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
                ) as MoonlightPlugin;

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

    public async Task CallPlugins(Func<MoonlightPlugin, Task> func)
    {
        foreach (var plugin in LoadedPlugins)
            await func.Invoke(plugin);
    }

    private void GetDllFiles(string path, List<string> result)
    {
        foreach (var directory in Directory.EnumerateDirectories(path))
            GetDllFiles(directory, result);

        foreach (var file in Directory.EnumerateFiles(path))
        {
            if (file.EndsWith(".dll"))
                result.Add(file);
        }
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