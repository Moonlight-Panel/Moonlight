using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using MoonCore.Logging;

namespace Moonlight.ApiServer.Startup;

public static partial class Startup
{
    private static void AddLogging(this WebApplicationBuilder builder)
    {
        // Logging providers
        builder.Logging.ClearProviders();
        
        builder.Logging.AddAnsiConsole();
        builder.Logging.AddFile(Path.Combine("storage", "logs", "moonlight.log"));

        // Logging levels
        var logConfigPath = Path.Combine("storage", "logConfig.json");

        // Ensure default log levels exist
        if (!File.Exists(logConfigPath))
        {
            var defaultLogLevels = new Dictionary<string, string>
            {
                { "Default", "Information" },
                { "Microsoft.AspNetCore", "Warning" },
                { "System.Net.Http.HttpClient", "Warning" },
                { "Moonlight.ApiServer.Implementations.LocalAuth.LocalAuthHandler", "Warning" }
            };

            var logLevelsJson = JsonSerializer.Serialize(defaultLogLevels);
            File.WriteAllText(logConfigPath, logLevelsJson);
        }

        // Read log levels
        var logLevels = JsonSerializer.Deserialize<Dictionary<string, string>>(
            File.ReadAllText(logConfigPath)
        )!;

        // Apply configured log levels
        foreach (var level in logLevels)
            builder.Logging.AddFilter(level.Key, Enum.Parse<LogLevel>(level.Value));

        // Mute exception handler middleware
        // https://github.com/dotnet/aspnetcore/issues/19740
        builder.Logging.AddFilter(
            "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware",
            LogLevel.Critical
        );

        builder.Logging.AddFilter(
            "Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware",
            LogLevel.Critical
        );
    }
}