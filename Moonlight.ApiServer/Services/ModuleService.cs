using System.Text.Json;
using MoonCore.Helpers;
using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Services;

public class ModuleService
{
    private readonly ILogger<ModuleService> Logger;
    private readonly Dictionary<string, ModuleModel> ModuleMeta = new();
    private static string ModulePath = PathBuilder.Dir("storage", "modules");

    public ModuleModel[] Modules => ModuleMeta.Values.ToArray();
    
    public ModuleService(ILogger<ModuleService> logger)
    {
        Logger = logger;
    }

    public void Load()
    {
        Logger.LogInformation("Loading modules");
        
        Directory.CreateDirectory(ModulePath);

        foreach (var moduleDirectory in Directory.EnumerateDirectories(ModulePath))
        {
            var metaPath = PathBuilder.File(moduleDirectory, "meta.json");
            
            if (!File.Exists(metaPath))
            {
                Logger.LogWarning("No meta.json found in {folder}", moduleDirectory);
                continue;
            }

            ModuleModel moduleModel;

            try
            {
                var json = File.ReadAllText(metaPath);
                moduleModel = JsonSerializer.Deserialize<ModuleModel>(json)!;
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while loading meta.json in {folder}: {e}", moduleDirectory, e);
                continue;
            }
            
            ModuleMeta.Add(moduleDirectory, moduleModel);
        }
        
        Logger.LogInformation("Loaded {count} modules", ModuleMeta.Count);
    }

    public string[] GetModuleDlls(string moduleName, string section)
    {
        if (ModuleMeta.All(x => x.Value.Name != moduleName))
            throw new ArgumentException($"No module with the name '{moduleName}' found");
        
        var moduleKvp = ModuleMeta
            .FirstOrDefault(x => x.Value.Name == moduleName);

        var module = moduleKvp.Value;

        if (!module.Modules.ContainsKey(section))
            return [];

        var modulePaths = module.Modules[section].Select(
            dllName => PathBuilder.File(ModulePath, moduleKvp.Key, "bin", section, dllName)
        ).Where(File.Exists).ToArray();

        return modulePaths;
    }
}