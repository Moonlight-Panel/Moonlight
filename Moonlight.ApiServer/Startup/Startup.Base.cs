using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoonCore.Extended.Extensions;
using MoonCore.Extensions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Plugins;

namespace Moonlight.ApiServer.Startup;

public partial class CleanStartup
{
    private Task RegisterBase(IPluginStartup[] pluginStartups)
    {
        WebApplicationBuilder.Services.AutoAddServices<CleanStartup>();
        WebApplicationBuilder.Services.AddHttpClient();

        WebApplicationBuilder.Services.AddApiExceptionHandler();

        // Add pre-existing services
        WebApplicationBuilder.Services.AddSingleton(Configuration);

        // Configure controllers
        var mvcBuilder = WebApplicationBuilder.Services.AddControllers();

        // Add plugin assemblies as application parts
        foreach (var pluginStartup in pluginStartups.Select(x => x.GetType().Assembly).Distinct())
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
}