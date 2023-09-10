using System.Reflection;
using System.Runtime.Loader;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.Plugin;
using Moonlight.App.Plugin.UI.Servers;
using Moonlight.App.Plugin.UI.Webspaces;

namespace Moonlight.App.Services.Plugins;

public class PluginService
{
    public readonly List<MoonlightPlugin> Plugins = new();
    public readonly Dictionary<MoonlightPlugin, string> PluginFiles = new();

    public Task ReloadPlugins()
    {
        PluginFiles.Clear();
        Plugins.Clear();
        
        // Try to update all plugins ending with .dll.cache
        foreach (var pluginFile in Directory.EnumerateFiles(
                         PathBuilder.Dir(Directory.GetCurrentDirectory(), "storage", "plugins"))
                     .Where(x => x.EndsWith(".dll.cache")))
        {
            try
            {
                var realPath = pluginFile.Replace(".cache", "");
                File.Copy(pluginFile, realPath, true);
                File.Delete(pluginFile);
                Logger.Info($"Updated plugin {realPath} on startup");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        var pluginType = typeof(MoonlightPlugin);
        
        foreach (var pluginFile in Directory.EnumerateFiles(
                     PathBuilder.Dir(Directory.GetCurrentDirectory(), "storage", "plugins"))
                     .Where(x => x.EndsWith(".dll")))
        {
            var assembly = Assembly.LoadFile(pluginFile);

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(pluginType))
                {
                    var plugin = (Activator.CreateInstance(type) as MoonlightPlugin)!;
                    
                    Logger.Info($"Loaded plugin '{plugin.Name}' ({plugin.Version}) by {plugin.Author}");
                    
                    Plugins.Add(plugin);
                    PluginFiles.Add(plugin, pluginFile);
                }
            }
        }
        
        Logger.Info($"Loaded {Plugins.Count} plugins");
        
        return Task.CompletedTask;
    }

    public async Task<ServerPageContext> BuildServerPage(ServerPageContext context)
    {
        foreach (var plugin in Plugins)
        {
            if (plugin.OnBuildServerPage != null)
                await plugin.OnBuildServerPage.Invoke(context);
        }

        return context;
    }
    
    public async Task<WebspacePageContext> BuildWebspacePage(WebspacePageContext context)
    {
        foreach (var plugin in Plugins)
        {
            if (plugin.OnBuildWebspacePage != null)
                await plugin.OnBuildWebspacePage.Invoke(context);
        }

        return context;
    }

    public async Task BuildServices(IServiceCollection serviceCollection)
    {
        foreach (var plugin in Plugins)
        {
            if (plugin.OnBuildServices != null)
                await plugin.OnBuildServices.Invoke(serviceCollection);
        }
    }

    public async Task<MalwareScan[]> BuildMalwareScans(MalwareScan[] defaultScans)
    {
        var scanList = defaultScans.ToList();

        foreach (var plugin in Plugins)
        {
            if (plugin.OnBuildMalwareScans != null)
                scanList = await plugin.OnBuildMalwareScans.Invoke(scanList);
        }

        return scanList.ToArray();
    }
}