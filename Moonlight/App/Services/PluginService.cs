using System.Reflection;
using Moonlight.App.Database.Entities;
using Moonlight.App.Helpers;
using Moonlight.App.Plugin;
using Moonlight.App.Plugin.UI;
using Moonlight.App.Plugin.UI.Servers;
using Moonlight.App.Plugin.UI.Webspaces;

namespace Moonlight.App.Services;

public class PluginService
{
    public List<MoonlightPlugin> Plugins { get; set; }

    public PluginService()
    {
        LoadPlugins();
    }
    
    private void LoadPlugins()
    {
        Plugins = new();

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
                }
            }
        }
        
        Logger.Info($"Loaded {Plugins.Count} plugins");
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
}