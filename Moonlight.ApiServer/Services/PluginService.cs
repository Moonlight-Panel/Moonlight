using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using MoonCore.Helpers;
using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Services;

public class PluginService
{
    private readonly ILogger<PluginService> Logger;
    private readonly string PluginRoot;

    public readonly Dictionary<PluginManifest, string> LoadedPlugins = new();
    public IFileProvider WwwRootFileProvider;

    public PluginService(ILogger<PluginService> logger)
    {
        Logger = logger;

        PluginRoot = PathBuilder.Dir("storage", "plugins");
    }

    public async Task Load()
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
        
        var pluginDirs = Directory.GetDirectories(PluginRoot);
        var pluginMap = new Dictionary<PluginManifest, string>();

        #region Scan plugins/ directory for plugin.json files

        foreach (var dir in pluginDirs)
        {
            var metaPath = PathBuilder.File(dir, "plugin.json");

            if (!File.Exists(metaPath))
            {
                Logger.LogWarning("Skipped '{dir}' as it is missing a plugin.json", dir);
                continue;
            }

            var json = await File.ReadAllTextAsync(metaPath);

            try
            {
                var meta = JsonSerializer.Deserialize<PluginManifest>(json, jsonOptions);

                if (meta == null)
                    throw new JsonException("Unable to parse. Return value was null");

                pluginMap.Add(meta, dir);
            }
            catch (JsonException e)
            {
                Logger.LogError("Unable to load plugin.json at '{path}': {e}", metaPath, e);
            }
        }

        #endregion

        #region Depdenency check

        foreach (var plugin in pluginMap.Keys)
        {
            var hasMissingDep = false;

            foreach (var dependency in plugin.Dependencies)
            {
                if (pluginMap.Keys.All(x => x.Id != dependency))
                {
                    hasMissingDep = true;
                    Logger.LogWarning("Plugin '{name}' has missing dependency: {dep}", plugin.Name, dependency);
                }
            }

            if (hasMissingDep)
                Logger.LogWarning("Unable to load '{name}' due to missing dependencies", plugin.Name);
            else
                LoadedPlugins.Add(plugin, pluginMap[plugin]);
        }

        #endregion

        #region Create wwwroot file provider

        Logger.LogInformation("Creating wwwroot file provider");
        WwwRootFileProvider = CreateWwwRootProvider();

        #endregion

        Logger.LogInformation("Loaded {count} plugins", LoadedPlugins.Count);
    }

    public Dictionary<string, string> GetAssemblies(string section)
    {
        var assemblyMap = new Dictionary<string, string>();

        foreach (var loadedPlugin in LoadedPlugins.Keys)
        {
            // Skip all plugins which haven't defined any assemblies in that section
            if (!loadedPlugin.Assemblies.ContainsKey(section))
                continue;

            var pluginPath = LoadedPlugins[loadedPlugin];

            foreach (var assembly in loadedPlugin.Assemblies[section])
            {
                var assemblyFile = Path.GetFileName(assembly);
                assemblyMap[assemblyFile] = PathBuilder.File(pluginPath, assembly);
            }
        }

        return assemblyMap;
    }

    private IFileProvider CreateWwwRootProvider()
    {
        List<IFileProvider> wwwRootProviders = new();

        foreach (var pluginFolder in LoadedPlugins.Values)
        {
            var wwwRootPath = Path.GetFullPath(
                PathBuilder.Dir(pluginFolder, "wwwroot")
            );

            wwwRootProviders.Add(
                new PhysicalFileProvider(wwwRootPath)
            );
        }

        return new CompositeFileProvider(wwwRootProviders);
    }
}