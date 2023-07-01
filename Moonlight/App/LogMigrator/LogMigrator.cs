using Moonlight.App.Helpers;

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
                Logger.Fatal(formatter(state, exception));
                
                if(exception != null)
                    Logger.Fatal(exception);
                
                break;
            case LogLevel.Warning:
                Logger.Warn(formatter(state, exception));
                
                if(exception != null)
                    Logger.Warn(exception);
                
                break;
            case LogLevel.Debug:
                Logger.Debug(formatter(state, exception));
                
                if(exception != null)
                    Logger.Debug(exception);
                
                break;
            case LogLevel.Error:
                Logger.Error(formatter(state, exception));
                
                if(exception != null)
                    Logger.Error(exception);
                
                break;
            case LogLevel.Information:
                Logger.Info(formatter(state, exception));
                
                if(exception != null)
                    Logger.Info(exception);
                
                break;
        }
    }
}