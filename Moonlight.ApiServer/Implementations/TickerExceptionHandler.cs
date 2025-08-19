using Microsoft.Extensions.Logging;
using TickerQ.Utilities.Enums;
using TickerQ.Utilities.Interfaces;

namespace Moonlight.ApiServer.Implementations;

public class TickerExceptionHandler : ITickerExceptionHandler
{
    private readonly ILogger<TickerExceptionHandler> Logger;

    public TickerExceptionHandler(ILogger<TickerExceptionHandler> logger)
    {
        Logger = logger;
    }

    public Task HandleExceptionAsync(Exception exception, Guid tickerId, TickerType tickerType)
    {
        Logger.LogError(exception, "An unhandled error occured while running ticker {id} ({type})", tickerId, tickerType);
        
        return Task.CompletedTask;
    }

    public Task HandleCanceledExceptionAsync(Exception exception, Guid tickerId, TickerType tickerType)
    {
        Logger.LogError(exception, "An unhandled error occured while handling canceled ticker {id} ({type})", tickerId, tickerType);
        
        return Task.CompletedTask;
    }
}