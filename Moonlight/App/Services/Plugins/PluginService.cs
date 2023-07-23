using System.Reflection;
using System.Runtime.Loader;
using Moonlight.App.Helpers;
using Moonlight.App.Plugin;
using Moonlight.App.Plugin.UI.Servers;
using Moonlight.App.Plugin.UI.Webspaces;

namespace Moonlight.App.Services.Plugins;

public class PluginService
{
    public List<MoonlightPlugin> Plugins { get; private set; }
    public Dictionary<MoonlightPlugin, string> PluginFiles { get; private set; }

    private AssemblyLoadContext LoadContext;

    public PluginService()
    {
        LoadContext = new(null, true);
        ReloadPlugins().Wait();
    }

    private Task UnloadPlugins()
    {
        Plugins = new();
        PluginFiles = new();
        
        if(LoadContext.Assemblies.Any())
            LoadContext.Unload();

        return Task.CompletedTask;
    }
    
    public async Task ReloadPlugins()
    {
        await UnloadPlugins();
        
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
            var assembly = LoadContext.LoadFromAssemblyPath(pluginFile);

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
}