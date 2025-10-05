using Hangfire;
using Hangfire.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moonlight.ApiServer.Database;

namespace Moonlight.ApiServer.Startup;

public static partial class Startup
{
    private static void AddMoonlightHangfire(this WebApplicationBuilder builder)
    {
        builder.Services.AddHangfire((provider, configuration) =>
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

        builder.Services.AddHangfireServer();

        builder.Logging.AddFilter(
            "Hangfire.Server.BackgroundServerProcess",
            LogLevel.Warning
        );

        builder.Logging.AddFilter(
            "Hangfire.BackgroundJobServer",
            LogLevel.Warning
        );
    }

    private static void UseMoonlightHangfire(this WebApplication application)
    {
        if (application.Environment.IsDevelopment())
            application.UseHangfireDashboard();
    }
}