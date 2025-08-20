using Microsoft.Extensions.DependencyInjection;
using MoonCore.Blazor.FlyonUi.Exceptions;
using MoonCore.Logging;
using Moonlight.Client.Implementations;

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

        WebAssemblyHostBuilder.Services.AddScoped<IGlobalErrorFilter, LogErrorFilter>();

        return Task.CompletedTask;
    }
}