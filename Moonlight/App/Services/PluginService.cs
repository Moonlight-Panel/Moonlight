using System.Reflection;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Abstractions;
using Moonlight.App.Models.Abstractions.Services;
using Moonlight.App.Plugins;
using Moonlight.App.Plugins.Contexts;

namespace Moonlight.App.Services;

public class PluginService
{
    private readonly List<MoonlightPlugin> Plugins = new();

    public async Task Load(WebApplicationBuilder webApplicationBuilder)
    {
        var path = PathBuilder.Dir("storage", "plugins");
        Directory.CreateDirectory(path);

        var files = FindFiles(path)
            .Where(x => x.EndsWith(".dll"))
            .ToArray();

        foreach (var file in files)
        {
            try
            {
                var assembly = Assembly.LoadFile(PathBuilder.File(Directory.GetCurrentDirectory(), file));

                int plugins = 0;
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(MoonlightPlugin)))
                    {
                        try
                        {
                            var plugin = (Activator.CreateInstance(type) as MoonlightPlugin)!;

                            // Create environment
                            plugin.Context = new PluginContext()
                            {
                                Services = webApplicationBuilder.Services,
                                WebApplicationBuilder = webApplicationBuilder
                            };

                            try
                            {
                                await plugin.Enable();
                                
                                // After here we can treat the plugin as successfully loaded
                                plugins++;
                                Plugins.Add(plugin);
                            }
                            catch (Exception e)
                            {
                                Logger.Fatal($"Unhandled exception while enabling plugin '{type.Name}'");
                                Logger.Fatal(e);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Fatal($"Failed to create plugin environment for '{type.Name}'");
                            Logger.Fatal(e);
                        }
                    }
                }
                
                if(plugins == 0) // If 0, we can assume that it was a library dll
                    Logger.Info($"Loaded {file} as a library");
                else
                    Logger.Info($"Loaded {plugins} plugin(s) from {file}");
            }
            catch (Exception e)
            {
                Logger.Fatal($"Unable to load assembly from file '{file}'");
                Logger.Fatal(e);
            }
        }
        
        Logger.Info($"Loaded {Plugins.Count} plugin(s)");
    }

    public async Task RunPreInit()
    {
        foreach (var plugin in Plugins)
        {
            Logger.Info($"Running pre init tasks for {plugin.GetType().Name}");

            foreach (var preInitTask in plugin.Context.PreInitTasks)
                await Task.Run(preInitTask);
        }
    }
    
    public async Task RunPrePost(WebApplication webApplication)
    {
        foreach (var plugin in Plugins)
        {
            // Pass through the dependency injection
            var scope = webApplication.Services.CreateScope();
            plugin.Context.Provider = scope.ServiceProvider;
            plugin.Context.Scope = scope;
            plugin.Context.WebApplication = webApplication;
            
            Logger.Info($"Running post init tasks for {plugin.GetType().Name}");

            foreach (var postInitTask in plugin.Context.PostInitTasks)
                await Task.Run(postInitTask);
        }
    }

    public Task BuildUserServiceView(ServiceViewContext context)
    {
        foreach (var plugin in Plugins)
        {
            plugin.Context.BuildUserServiceView?.Invoke(context);
        }
        
        return Task.CompletedTask;
    }
    
    public Task BuildAdminServiceView(ServiceViewContext context)
    {
        foreach (var plugin in Plugins)
        {
            plugin.Context.BuildAdminServiceView?.Invoke(context);
        }
        
        return Task.CompletedTask;
    }

    private string[] FindFiles(string dir)
    {
        var result = new List<string>();

        foreach (var file in Directory.GetFiles(dir))
            result.Add(file);

        foreach (var directory in Directory.GetDirectories(dir))
        {
            result.AddRange(FindFiles(directory));
        }

        return result.ToArray();
    }
}