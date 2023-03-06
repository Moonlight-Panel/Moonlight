using System.Collections.Concurrent;

namespace Moonlight.App.LogMigrator;

public class LogMigratorProvider  : ILoggerProvider
{
    public LogMigratorProvider() { }
    public LogMigratorProvider(EventHandler onCreateLogger)
    {
        OnCreateLogger = onCreateLogger;
    }

    private ConcurrentDictionary<string, App.LogMigrator.LogMigrator> Loggers { get; set; } = new();

    public ILogger CreateLogger(string categoryName)
    {
        App.LogMigrator.LogMigrator customLogger = Loggers.GetOrAdd(categoryName, new App.LogMigrator.LogMigrator(categoryName));
        OnCreateLogger?.Invoke(this, new LogMigratorProviderEventArgs(customLogger));
        return customLogger;
    }

    public void Dispose() { }

    public event EventHandler OnCreateLogger = delegate { };

    private class LogMigratorProviderEventArgs : EventArgs
    {
        private App.LogMigrator.LogMigrator CustomLogger { get; }
        public LogMigratorProviderEventArgs(App.LogMigrator.LogMigrator logger)
        {
            CustomLogger = logger;
        }
    }
}