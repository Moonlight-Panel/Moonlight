using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moonlight.ApiServer.Http.Hubs;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    public Task RegisterSignalRAsync()
    {
        var signalRBuilder = WebApplicationBuilder.Services.AddSignalR();

        if (Configuration.SignalR.UseRedis)
            signalRBuilder.AddStackExchangeRedis(Configuration.SignalR.RedisConnectionString);
        
        return Task.CompletedTask;
    }

    public Task MapSignalRAsync()
    {
        WebApplication.MapHub<DiagnoseHub>("/api/admin/system/diagnose/ws");
        
        return Task.CompletedTask;
    }
}