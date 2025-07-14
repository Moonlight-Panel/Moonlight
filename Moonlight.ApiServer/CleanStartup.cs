using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Plugins;

namespace Moonlight.ApiServer;

public class CleanStartup
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
    
    #region Base

    private Task RegisterBase()
    {
        WebApplicationBuilder.Services.AutoAddServices<Startup>();
        WebApplicationBuilder.Services.AddHttpClient();

        WebApplicationBuilder.Services.AddApiExceptionHandler();

        // Add pre-existing services
        WebApplicationBuilder.Services.AddSingleton(Configuration);

        // Configure controllers
        var mvcBuilder = WebApplicationBuilder.Services.AddControllers();

        // Add plugin assemblies as application parts
        foreach (var pluginStartup in PluginStartups.Select(x => x.GetType().Assembly).Distinct())
            mvcBuilder.AddApplicationPart(pluginStartup.GetType().Assembly);

        return Task.CompletedTask;
    }

    private Task UseBase()
    {
        WebApplication.UseRouting();
        WebApplication.UseExceptionHandler();

        if (Configuration.Client.Enable)
        {
            if (WebApplication.Environment.IsDevelopment())
                WebApplication.UseWebAssemblyDebugging();

            WebApplication.UseBlazorFrameworkFiles();
            WebApplication.UseStaticFiles();
        }

        return Task.CompletedTask;
    }

    private Task MapBase()
    {
        WebApplication.MapControllers();

        if (Configuration.Client.Enable)
            WebApplication.MapFallbackToController("Index", "Frontend");

        return Task.CompletedTask;
    }

    private Task ConfigureKestrel()
    {
        WebApplicationBuilder.WebHost.ConfigureKestrel(kestrelOptions =>
        {
            var maxUploadInBytes = ByteConverter
                .FromMegaBytes(Configuration.Kestrel.UploadLimit)
                .Bytes;

            kestrelOptions.Limits.MaxRequestBodySize = maxUploadInBytes;
        });

        return Task.CompletedTask;
    }

    #endregion
}