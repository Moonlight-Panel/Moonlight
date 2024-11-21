using System.Text.Json;
using MoonCore.Helpers;
using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Services;

public class PluginService
{
    public readonly List<PluginMeta> Plugins = new();
    
    private static string PluginsFolder = PathBuilder.Dir("storage", "plugins");
    private readonly ILogger<PluginService> Logger;

    private readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PluginService(ILogger<PluginService> logger)
    {
        Logger = logger;
    }

    public async Task Load()
    {
        // Load all manifest files
        foreach (var pluginFolder in Directory.EnumerateDirectories(PluginsFolder))
        {
            var manifestPath = PathBuilder.File(pluginFolder, "plugin.json");

            if (!File.Exists(manifestPath))
            {
                Logger.LogWarning("Ignoring '{folder}' because no manifest has been found", pluginFolder);
                continue;
            }

            PluginManifest manifest;

            try
            {
                var manifestText = await File.ReadAllTextAsync(manifestPath);
                manifest = JsonSerializer.Deserialize<PluginManifest>(manifestText, SerializerOptions)!;
            }
            catch (Exception e)
            {
                Logger.LogError("An unhandled error occured while loading plugin manifest in '{folder}': {e}",
                    pluginFolder, e);
                break;
            }

            Logger.LogTrace("Loaded plugin manifest. Id: {id}", manifest.Id);

            Plugins.Add(new()
            {
                Manifest = manifest,
                Path = pluginFolder
            });
        }

        // Check for missing dependencies
        var pluginsToNotLoad = new List<string>();

        foreach (var plugin in Plugins)
        {
            foreach (var dependency in plugin.Manifest.Dependencies)
            {
                // Check if dependency is found
                if (Plugins.Any(x => x.Manifest.Id == dependency))
                    continue;

                Logger.LogError(
                    "Unable to load plugin '{id}' ({path}) because the dependency {dependency} is missing",
                    plugin.Manifest.Id,
                    plugin.Path,
                    dependency
                );
                
                pluginsToNotLoad.Add(plugin.Manifest.Id);
                break;
            }
        }
        
        // Remove unloadable plugins from cache
        Plugins.RemoveAll(x => pluginsToNotLoad.Contains(x.Manifest.Id));
    }

    public Dictionary<string, string> GetAssemblies(string section)
    {
        var pathMappings = new Dictionary<string, string>();

        foreach (var plugin in Plugins)
        {
            foreach (var file in Directory.EnumerateFiles(PathBuilder.Dir(plugin.Path, "bin", section)))
            {
                if(!file.EndsWith(".dll"))
                    continue;

                var fileName = Path.GetFileName(file);
                pathMappings[fileName] = file;
            }
        }

        return pathMappings;
    }

    public string[] GetEntrypoints(string section)
    {
        return Plugins
            .Where(x => x.Manifest.Entrypoints.ContainsKey(section))
            .SelectMany(x => x.Manifest.Entrypoints[section])
            .ToArray();
    }
}