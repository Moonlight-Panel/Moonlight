using Microsoft.AspNetCore.Builder;
using Moonlight.ApiServer.Plugins;

namespace Moonlight.ApiServer.Startup;

public static partial class Startup
{
    public static void AddMoonlight(this WebApplicationBuilder builder, IPluginStartup[] startups)
    {
        PrintVersionAsync();
        CreateStorageAsync();
        
        builder.AddConfiguration();
        builder.AddLogging();
        
        builder.ConfigureKestrel();
        builder.AddBase(startups);
        builder.AddDatabase();
        builder.AddAuth();
        builder.AddMoonlightCors();
        builder.AddMoonlightHangfire();
        builder.AddMoonlightSignalR();
        
        builder.AddPlugins(startups);
    }

    public static void UseMoonlight(this WebApplication application, IPluginStartup[] startups)
    {
        application.UseBase();
        application.UseMoonlightCors();
        application.UseAuth();
        application.UseMoonlightHangfire();
        application.UsePlugins(startups);
    }

    public static void MapMoonlight(this WebApplication application, IPluginStartup[] startups)
    {
        application.MapBase();
        application.MapMoonlightSignalR();
        application.MapPlugins(startups);
    }
}