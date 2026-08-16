using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Moonlight.Frontend.Infrastructure.Helpers;

public class AppConsoleFormatter : ConsoleFormatter
{
    private const string TimestampColor = "\e[38;2;148;148;148m";
    private const string CategoryColor = "\e[38;2;198;198;198m";
    private const string MessageColor = "\e[38;2;255;255;255m";
    private const string Bold = "\e[1m";

    private const string CriticalColor = "\e[38;2;255;0;0m";
    private const string ErrorColor = "\e[38;2;255;0;0m";
    private const string WarningColor = "\e[38;2;215;215;0m";
    private const string InfoColor = "\e[38;2;135;215;255m";
    private const string DebugColor = "\e[38;2;198;198;198m";
    private const string TraceColor = "\e[38;2;68;68;68m";

    public AppConsoleFormatter() : base(nameof(AppConsoleFormatter))
    {
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter
    )
    {
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

        // Timestamp
        textWriter.Write(TimestampColor);
        textWriter.Write(DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
        textWriter.Write(' ');

        // Log level with color and bold
        var (levelText, levelColor) = GetLevelInfo(logEntry.LogLevel);
        textWriter.Write(levelColor);
        textWriter.Write(Bold);
        textWriter.Write(levelText);
        textWriter.Write(' ');

        // Category
        textWriter.Write(CategoryColor);
        textWriter.Write(logEntry.Category);

        // Message
        textWriter.Write(MessageColor);
        textWriter.Write(": ");
        textWriter.Write(message);

        // Exception
        if (logEntry.Exception != null)
        {
            textWriter.Write(MessageColor);
            textWriter.WriteLine(logEntry.Exception.ToString());
        }
        else
        {
            textWriter.WriteLine();
        }
    }

    private static (string text, string color) GetLevelInfo(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Critical => ("CRIT", CriticalColor),
            LogLevel.Error => ("ERRO", ErrorColor),
            LogLevel.Warning => ("WARN", WarningColor),
            LogLevel.Information => ("INFO", InfoColor),
            LogLevel.Debug => ("DEBG", DebugColor),
            LogLevel.Trace => ("TRCE", TraceColor),
            _ => ("NONE", "")
        };
    }
}