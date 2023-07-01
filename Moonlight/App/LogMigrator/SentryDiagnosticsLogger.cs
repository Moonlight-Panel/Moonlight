using Moonlight.App.Helpers;
using Sentry;
using Sentry.Extensibility;

namespace Moonlight.App.LogMigrator;

public class SentryDiagnosticsLogger : IDiagnosticLogger
{
    private readonly SentryLevel Level;

    public SentryDiagnosticsLogger(SentryLevel level)
    {
        Level = level;
    }

    public bool IsEnabled(SentryLevel level)
    {
        if ((int)level >= (int)Level)
        {
            return true;
        }

        return false;
    }

    public void Log(SentryLevel logLevel, string message, Exception? exception = null, params object?[] args)
    {
        switch (logLevel)
        {
            case SentryLevel.Debug:
                Logger.Debug(string.Format(message, args));
                
                if(exception != null)
                    Logger.Debug(exception);
                
                break;
            
            case SentryLevel.Info:
                Logger.Info(string.Format(message, args));
                
                if(exception != null)
                    Logger.Info(exception);
                
                break;
            
            case SentryLevel.Warning:
                Logger.Warn(string.Format(message, args));
                
                if(exception != null)
                    Logger.Warn(exception);
                
                break;
            
            case SentryLevel.Error:
                Logger.Error(string.Format(message, args));
                
                if(exception != null)
                    Logger.Error(exception);
                
                break;
            
            case SentryLevel.Fatal:
                Logger.Fatal(string.Format(message, args));
                
                if(exception != null)
                    Logger.Fatal(exception);
                
                break;
        }
    }
}