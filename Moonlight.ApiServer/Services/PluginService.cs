using System.Text.Json;
using MoonCore.Helpers;
using MoonCore.Models;
using Moonlight.ApiServer.Models;
using Moonlight.Shared.Http.Responses.PluginsStream;

namespace Moonlight.ApiServer.Services;

public class PluginService
{
    public readonly List<PluginMeta> Plugins = new();
    public readonly Dictionary<string, string> AssetMap = new();
    public HostedPluginsManifest HostedPluginsManifest;
    public PluginsAssetManifest PluginsAssetManifest;
    public Dictionary<string, string> AssemblyMap;

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
        
        // Generate assembly map for client
        AssemblyMap = GetAssemblies("client");
        
        // Generate plugin stream manifest for client
        HostedPluginsManifest = new()
        {
            Assemblies = AssemblyMap.Keys.ToArray(),
            Entrypoints = GetEntrypoints("client")
        };
        
        // Generate asset map
        GenerateAssetMap();
        
        // Generate asset manifest
        PluginsAssetManifest = new()
        {
            CssFiles = AssetMap.Keys.Where(x => x.EndsWith(".css")).ToArray(),
            JavascriptFiles = AssetMap.Keys.Where(x => x.EndsWith(".js")).ToArray(),
        };
    }

    public Dictionary<string, string> GetAssemblies(string section)
    {
        var pathMappings = new Dictionary<string, string>();

        foreach (var plugin in Plugins)
        {
            foreach (var file in Directory.EnumerateFiles(PathBuilder.Dir(plugin.Path, "bin", section)))
            {
                if (!file.EndsWith(".dll"))
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

    private void GenerateAssetMap()
    {
        AssetMap.Clear();

        foreach (var plugin in Plugins)
        {
            var assetPath = PathBuilder.Dir(plugin.Path, "wwwroot");

            if (!Directory.Exists(assetPath))
                continue;

            var files = new List<string>();
            GetFilesInDirectory(assetPath, files);

            foreach (var file in files)
            {
                var mapPath = Formatter.ReplaceStart(file, assetPath, "");

                mapPath = mapPath.Replace("\\", "/"); // To handle fucking windows
                mapPath = mapPath.StartsWith("/") ? mapPath : "/" + mapPath; // Ensure starting /

                if (AssetMap.ContainsKey(mapPath))
                {
                    Logger.LogWarning(
                        "The plugin '{name}' tries to map an asset to the path '{path}' which is already used by another plugin. Ignoring asset mapping",
                        plugin.Manifest.Id,
                        mapPath
                    );
                    
                    continue;
                }

                AssetMap[mapPath] = file;
            }
        }
    }

    private void GetFilesInDirectory(string directory, List<string> files)
    {
        files.AddRange(Directory.EnumerateFiles(directory));

        foreach (var dir in Directory.EnumerateDirectories(directory))
            GetFilesInDirectory(dir, files);
    }
}