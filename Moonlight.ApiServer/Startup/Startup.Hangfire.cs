using Hangfire;
using Hangfire.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moonlight.ApiServer.Database;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private Task RegisterHangfireAsync()
    {
        WebApplicationBuilder.Services.AddHangfire((provider, configuration) =>
        {
            configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
            configuration.UseSimpleAssemblyNameTypeSerializer();
            configuration.UseRecommendedSerializerSettings();
            configuration.UseEFCoreStorage(() =>
            {
                var scope = provider.CreateScope();
                return scope.ServiceProvider.GetRequiredService<CoreDataContext>();
            }, new EFCoreStorageOptions());
        });

        WebApplicationBuilder.Services.AddHangfireServer();

        WebApplicationBuilder.Logging.AddFilter(
            "Hangfire.Server.BackgroundServerProcess",
            LogLevel.Warning
        );

        WebApplicationBuilder.Logging.AddFilter(
            "Hangfire.BackgroundJobServer",
            LogLevel.Warning
        );

        return Task.CompletedTask;
    }

    private Task UseHangfireAsync()
    {
        if (WebApplication.Environment.IsDevelopment())
            WebApplication.UseHangfireDashboard();

        return Task.CompletedTask;
    }
}