using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoonCore.Extended.Extensions;
using MoonCore.Extensions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Plugins;

namespace Moonlight.ApiServer.Startup;

public static partial class Startup
{
    private static void AddBase(this WebApplicationBuilder builder, IPluginStartup[] startups)
    {
        builder.Services.AutoAddServices<IAssemblyMarker>();
        builder.Services.AddHttpClient();

        builder.Services.AddApiExceptionHandler();

        // Configure controllers
        var mvcBuilder = builder.Services.AddControllers();

        // Add plugin assemblies as application parts
        foreach (var pluginStartup in startups.Select(x => x.GetType().Assembly).Distinct())
            mvcBuilder.AddApplicationPart(pluginStartup.GetType().Assembly);
    }

    private static void UseBase(this WebApplication application)
    {
        application.UseRouting();
        application.UseExceptionHandler();
    }

    private static void MapBase(this WebApplication application)
    {
        application.MapControllers();
        
        // Frontend
        var configuration = AppConfiguration.CreateEmpty();
        application.Configuration.Bind(configuration);
        
        if (configuration.Frontend.EnableHosting)
            application.MapFallbackToController("Index", "Frontend");
    }

    private static void ConfigureKestrel(this WebApplicationBuilder builder)
    {
        var configuration = AppConfiguration.CreateEmpty();
        builder.Configuration.Bind(configuration);
        
        builder.WebHost.ConfigureKestrel(kestrelOptions =>
        {
            var maxUploadInBytes = ByteConverter
                .FromMegaBytes(configuration.Kestrel.UploadLimit)
                .Bytes;

            kestrelOptions.Limits.MaxRequestBodySize = maxUploadInBytes;
        });
    }
}