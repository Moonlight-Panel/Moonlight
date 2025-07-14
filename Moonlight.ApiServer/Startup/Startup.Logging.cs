using System.Text.Json;
using Microsoft.Extensions.Logging;
using MoonCore.Logging;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private Task SetupLogging()
    {
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddAnsiConsole();

        Logger = loggerFactory.CreateLogger<Startup>();

        return Task.CompletedTask;
    }

    private async Task RegisterLogging()
    {
        // Configure application logging
        WebApplicationBuilder.Logging.ClearProviders();
        WebApplicationBuilder.Logging.AddAnsiConsole();
        WebApplicationBuilder.Logging.AddFile(Path.Combine("storage", "logs", "moonlight.log"));

        // Logging levels
        var logConfigPath = Path.Combine("storage", "logConfig.json");

        // Ensure logging config, add a default one is missing
        if (!File.Exists(logConfigPath))
        {
            var defaultLogLevels = new Dictionary<string, string>
            {
                { "Default", "Information" },
                { "Microsoft.AspNetCore", "Warning" },
                { "System.Net.Http.HttpClient", "Warning" }
            };

            var logLevelsJson = JsonSerializer.Serialize(defaultLogLevels);
            await File.WriteAllTextAsync(logConfigPath, logLevelsJson);
        }

        // Add logging configuration
        var logLevels = JsonSerializer.Deserialize<Dictionary<string, string>>(
            await File.ReadAllTextAsync(logConfigPath)
        )!;

        foreach (var level in logLevels)
            WebApplicationBuilder.Logging.AddFilter(level.Key, Enum.Parse<LogLevel>(level.Value));

        // Mute exception handler middleware
        // https://github.com/dotnet/aspnetcore/issues/19740
        WebApplicationBuilder.Logging.AddFilter(
            "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware",
            LogLevel.Critical
        );

        WebApplicationBuilder.Logging.AddFilter(
            "Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware",
            LogLevel.Critical
        );
    }
}