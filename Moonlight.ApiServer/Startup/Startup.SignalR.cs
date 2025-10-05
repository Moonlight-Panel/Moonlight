using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Http.Hubs;

namespace Moonlight.ApiServer.Startup;

public static partial class Startup
{
    private static void AddMoonlightSignalR(this WebApplicationBuilder builder)
    {
        var configuration = AppConfiguration.CreateEmpty();
        builder.Configuration.Bind(configuration);
        
        var signalRBuilder = builder.Services.AddSignalR();

        if (configuration.SignalR.UseRedis)
            signalRBuilder.AddStackExchangeRedis(configuration.SignalR.RedisConnectionString);
    }

    private static void MapMoonlightSignalR(this WebApplication application)
    {
        application.MapHub<DiagnoseHub>("/api/admin/system/diagnose/ws");
    }
}