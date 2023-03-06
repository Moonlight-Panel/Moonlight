using Logging.Net;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Moonlight.App.LogMigrator;

// This is used to migrate microsoft logging to logging.net
public class LogMigrator : ILogger
{
    private string Name;

    public LogMigrator(string name)
    {
        Name = name;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        switch (logLevel)
        {
            case LogLevel.Critical:
                Logger.Fatal($"[{Name}] {formatter(state, exception)}");
                break;
            case LogLevel.Warning:
                Logger.Warn($"[{Name}] {formatter(state, exception)}");
                break;
            case LogLevel.Debug:
                Logger.Debug($"[{Name}] {formatter(state, exception)}");
                break;
            case LogLevel.Error:
                Logger.Error($"[{Name}] {formatter(state, exception)}");
                break;
            case LogLevel.Information:
                Logger.Info($"[{Name}] {formatter(state, exception)}");
                break;
        }
    }
}