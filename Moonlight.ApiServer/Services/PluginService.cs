using System.Text.Json;
using MoonCore.Helpers;
using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Services;

public class PluginService
{
    private readonly Dictionary<string, PluginMeta> Plugins = new();
    private readonly ILogger<PluginService> Logger;

    private static string PluginsPath = PathBuilder.Dir("storage", "plugins");

    public PluginService(ILogger<PluginService> logger)
    {
        Logger = logger;
    }

    public async Task Load()
    {
        Logger.LogInformation("Loading plugins...");

        foreach (var pluginPath in Directory.EnumerateDirectories(PluginsPath))
        {
            var metaPath = PathBuilder.File(PluginsPath, "meta.json");

            if (!File.Exists(metaPath))
            {
                Logger.LogWarning("Ignoring folder '{name}'. No meta.json found", pluginPath);
                continue;
            }

            var metaContent = await File.ReadAllTextAsync(metaPath);
            PluginMeta meta;

            try
            {
                meta = JsonSerializer.Deserialize<PluginMeta>(metaContent)
                       ?? throw new JsonException();
            }
            catch (Exception e)
            {
                Logger.LogCritical("Unable to deserialize meta.json: {e}", e);
                continue;
            }
            
            Plugins.Add(pluginPath, meta);
        }
    }

    public Task<Dictionary<string, string[]>> GetBinariesBySection(string section)
    {
        var bins = Plugins
            .Values
            .Where(x => x.Binaries.ContainsKey(section))
            .ToDictionary(x => x.Id, x => x.Binaries[section]);

        return Task.FromResult(bins);
    }

    public Task<Stream?> GetBinaryStream(string plugin, string section, string fileName)
    {
        if (Plugins.All(x => x.Value.Id != plugin))
            return Task.FromResult<Stream?>(null);

        var pluginData = Plugins.First(x => x.Value.Id == plugin);
        var binaryPath = PathBuilder.File(pluginData.Key, section, fileName);

        if (!File.Exists(binaryPath))
            return Task.FromResult<Stream?>(null);

        var fs = File.OpenRead(binaryPath);
        return Task.FromResult<Stream?>(fs);
    }
}