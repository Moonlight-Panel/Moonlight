using System.Reflection;
using MoonCore.Helpers;

namespace Moonlight.ApiServer.App.PluginApi;

public class PluginService
{
    private readonly ILogger<PluginService> Logger;
    private readonly LoggerFactory LoggerFactory;

    public readonly List<Assembly> PluginAssemblies = new();
    public readonly List<Assembly> LibraryAssemblies = new();
    public readonly List<MoonlightPlugin> LoadedPlugins = new();

    public PluginService(ILogger<PluginService> logger, LoggerFactory loggerFactory)
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

        foreach (var dllFile in dllFilesInFolder)
        {
            try
            {
                var assembly = Assembly.LoadFile(
                    Path.GetFullPath(dllFile)
                );

                var plugins = assembly.ExportedTypes
                    .Where(x => x.IsSubclassOf(pluginType))
                    .ToArray();

                if (plugins.Length == 0)
                {
                    Logger.LogInformation("Loaded '{file}' as library", dllFile);
                    LibraryAssemblies.Add(assembly);
                }
                else
                {
                    pluginTypes.AddRange(plugins);
                    PluginAssemblies.Add(assembly);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Unable to load the dll file '{file}' because an error occured: {e}", dllFile, e);
            }
        }

        Logger.LogInformation("Initializing plugins");
        foreach (var type in pluginTypes)
        {
            try
            {
                var ctrors = type.GetConstructors();
                
                var pl = ctrors.First().Invoke([
                    LoggerFactory.CreateLogger(type),
                    this
                ]) as MoonlightPlugin;
                
                /*
                var pl = Activator.CreateInstance(type, [
                        LoggerFactory.CreateLogger(type),
                        this
                    ]
                ) as MoonlightPlugin;*/

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
}