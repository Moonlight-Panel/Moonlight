using System.Reflection;
using MoonCore.Helpers;
using Moonlight.Core.Models.Abstractions.Plugins;

namespace Moonlight.Core.Services;

public class PluginService
{
    private readonly Dictionary<Type, List<object>> ImplementationCache = new();
    private readonly List<MoonlightPlugin> Plugins = new();

    private readonly ILogger<PluginService> Logger;

    public PluginService(ILogger<PluginService> logger)
    {
        Logger = logger;
        
        Directory.CreateDirectory(PathBuilder.Dir("storage", "plugins"));
    }

    public async Task PreInitialize(WebApplicationBuilder builder)
    {
        MoonlightPlugin[] plugins;

        lock (Plugins)
            plugins = Plugins.ToArray();
        
        foreach (var plugin in plugins)
            await plugin.OnPreInitialized(builder);
    }
    
    public async Task Initialized(WebApplication application)
    {
        MoonlightPlugin[] plugins;

        lock (Plugins)
            plugins = Plugins.ToArray();
        
        foreach (var plugin in plugins)
            await plugin.OnInitialized(application);
    }

    public Task RegisterImplementation<T>(T implementation) where T : class
    {
        var type = typeof(T);
        
        lock (ImplementationCache)
        {
            if(!ImplementationCache.ContainsKey(type))
                ImplementationCache.Add(type, new());
            
            ImplementationCache[type].Add(implementation!);
        }
        
        return Task.CompletedTask;
    }

    public Task<T[]> GetImplementations<T>() where T : class
    {
        var type = typeof(T);
        T[] result;
        
        lock (ImplementationCache)
        {
            if(!ImplementationCache.ContainsKey(type))
                ImplementationCache.Add(type, new());

            result = ImplementationCache[type]
                .Select(x => (x as T)!)
                .ToArray();
        }

        return Task.FromResult(result);
    }

    public async Task ExecuteFuncFunction<T>(Action<T> function) where T : class
    {
        var implementations = await GetImplementations<T>();

        foreach (var implementation in implementations)
            function.Invoke(implementation);
    }

    public async Task ExecuteFuncAsync<T>(Func<T, Task> function) where T : class
    {
        var implementations = await GetImplementations<T>();

        foreach (var implementation in implementations)
            await function.Invoke(implementation);
    }
    
    public async Task<TResult[]> ExecuteFuncAsync<T, TResult>(Func<T, Task<TResult>> function) where T : class
    {
        var implementations = await GetImplementations<T>();
        List<TResult> results = new();

        foreach (var implementation in implementations)
        {
            var res = await function.Invoke(implementation);
            results.Add(res);
        }

        return results.ToArray();
    }

    public async Task<MoonlightPlugin> LoadFromType<T>() where T : MoonlightPlugin
    {
        return await LoadFromType(typeof(T));
    }
    
    public Task<MoonlightPlugin> LoadFromType(Type type)
    {
        var plugin = Activator.CreateInstance(type) as MoonlightPlugin;

        if (plugin == null)
            throw new ArgumentException($"{type.FullName} is not a MoonlightPlugin");
        
        lock (Plugins)
            Plugins.Add(plugin);

        plugin.Plugin = this;
        
        return Task.FromResult(plugin);
    }

    public async Task Load()
    {
        var dllFiles = FindDllFiles(PathBuilder.Dir("storage", "plugins"));

        foreach (var dllFile in dllFiles)
        {
            try
            {
                var assembly = Assembly.LoadFile(Path.GetFullPath(dllFile));

                var pluginTypes = assembly
                    .GetTypes()
                    .Where(x => x.IsSubclassOf(typeof(MoonlightPlugin)))
                    .ToArray();

                if (pluginTypes.Length == 0)
                {
                    Logger.LogInformation("Loaded assembly as library: {dllFile}", dllFile);
                    continue;
                }

                foreach (var pluginType in pluginTypes)
                {
                    try
                    {
                        var plugin = await LoadFromType(pluginType);
                        
                        Logger.LogInformation("Loaded plugin '{name}'. Created by '{author}'", plugin.Name, plugin.Author);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("An error occured while loading plugin '{name}': {e}", pluginType.FullName, e);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while loading assembly '{dllFile}': {e}", dllFile, e);
            }
        }
    }

    public Task<MoonlightPlugin[]> GetLoadedPlugins()
    {
        lock (Plugins)
            return Task.FromResult(Plugins.ToArray());
    }

    private string[] FindDllFiles(string folder)
    {
        var result = new List<string>();

        var files = Directory.GetFiles(folder).Where(x => x.EndsWith(".dll"));
        result.AddRange(files);

        foreach (var directory in Directory.GetDirectories(folder))
            result.AddRange(FindDllFiles(directory));

        return result.ToArray();
    }
}