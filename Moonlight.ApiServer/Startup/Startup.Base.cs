using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MoonCore.Extended.Extensions;
using MoonCore.Extensions;
using MoonCore.Helpers;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
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

        return Task.CompletedTask;
    }

    private Task MapBase()
    {
        WebApplication.MapControllers();

        if (Configuration.Frontend.EnableHosting)
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
}