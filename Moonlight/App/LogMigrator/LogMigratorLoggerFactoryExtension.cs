namespace Moonlight.App.LogMigrator;

public static class LogMigratorLoggerFactoryExtensions
{
    public static ILoggerFactory AddCustomLogger(
        this ILoggerFactory factory, out LogMigratorProvider logProvider)
    {
        logProvider = new LogMigratorProvider();
        factory.AddProvider(logProvider);
        return factory;
    }
}