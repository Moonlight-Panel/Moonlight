using MoonCore.Logging;

namespace Moonlight.Client.Startup;

public partial class Startup
{
    private Task SetupLogging()
    {
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddAnsiConsole();

        Logger = loggerFactory.CreateLogger<Startup>();

        return Task.CompletedTask;
    }

    private Task RegisterLogging()
    {
        WebAssemblyHostBuilder.Logging.ClearProviders();
        WebAssemblyHostBuilder.Logging.AddAnsiConsole();

        return Task.CompletedTask;
    }
}