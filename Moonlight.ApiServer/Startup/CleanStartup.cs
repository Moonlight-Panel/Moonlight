using Microsoft.AspNetCore.Builder;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Plugins;

namespace Moonlight.ApiServer.Startup;

public partial class CleanStartup
{
    // Logger
    private ILogger Logger;
    
    // Configuration
    private AppConfiguration Configuration;
    
    // WebApplication Stuff
    private WebApplication WebApplication;
    private WebApplicationBuilder WebApplicationBuilder;
    
    public async Task AddMoonlight(
        WebApplicationBuilder builder,
        string[] args,
        IPluginStartup[]? plugins = null
    )
    {
        
    }

    public async Task AddMoonlight(
        WebApplication application,
        string[] args,
        IPluginStartup[]? plugins = null
    )
    {
    }

    #region Misc

    private Task PrintVersion()
    {
        // Fancy start console output... yes very fancy :>
        var rainbow = new Crayon.Rainbow(0.5);
        foreach (var c in "Moonlight")
        {
            Console.Write(
                rainbow
                    .Next()
                    .Bold()
                    .Text(c.ToString())
            );
        }

        Console.WriteLine();

        return Task.CompletedTask;
    }

    private Task CreateStorage()
    {
        Directory.CreateDirectory("storage");
        Directory.CreateDirectory(Path.Combine("storage", "logs"));

        return Task.CompletedTask;
    }

    #endregion
}