using MoonCore.Blazor.FlyonUi.Exceptions;

namespace Moonlight.Client.Implementations;

public class LogErrorFilter : IGlobalErrorFilter
{
    private readonly ILogger<LogErrorFilter> Logger;

    public LogErrorFilter(ILogger<LogErrorFilter> logger)
    {
        Logger = logger;
    }

    public Task<bool> HandleException(Exception ex)
    {
        Logger.LogError(ex, "Global error processed");
        return Task.FromResult(false);
    }
}