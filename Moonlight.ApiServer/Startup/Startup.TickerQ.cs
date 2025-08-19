using Microsoft.Extensions.DependencyInjection;
using Moonlight.ApiServer.Database;
using Moonlight.ApiServer.Implementations;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private Task RegisterTickerQ()
    {
        WebApplicationBuilder.Services.AddTickerQ(builder =>
        {
            builder.SetExceptionHandler<TickerExceptionHandler>();

            builder.AddOperationalStore<TickerDataContext>(optionBuilder =>
            {
                optionBuilder.CancelMissedTickersOnApplicationRestart();
            });
        });

        WebApplicationBuilder.Services.AddDbContext<TickerDataContext>();
        
        return Task.CompletedTask;
    }

    private Task UseTickerQ()
    {
        WebApplication.UseTickerQ();
        
        return Task.CompletedTask;
    }
}