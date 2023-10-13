namespace Moonlight.App.Helpers.LogMigrator;

public class LogMigrateProvider : ILoggerProvider
{
    public void Dispose() {}

    public ILogger CreateLogger(string categoryName)
    {
        return new MigrateLogger();
    }
}