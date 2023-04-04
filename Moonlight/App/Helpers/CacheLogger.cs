using System.Diagnostics;
using Logging.Net;
using Logging.Net.Loggers.SB;
using Moonlight.App.Models.Misc;
using ILogger = Logging.Net.ILogger;

namespace Moonlight.App.Helpers;

public class CacheLogger : ILogger
{
    private SBLogger SbLogger = new();
    private List<LogEntry> Messages = new();

    public LogEntry[] GetMessages()
    {
        lock (Messages)
        {
            var result = new LogEntry[Messages.Count];
            Messages.CopyTo(result);
            return result;
        }
    }

    public void Clear(int messages)
    {
        lock (Messages)
        {
            Messages.RemoveRange(0, Math.Min(messages, Messages.Count));
        }
    }

    public void Info(string? s)
    {
        if (s == null)
            return;

        lock (Messages)
        {
            Messages.Add(new()
            {
                Level = "info",
                Message = s
            });
        }

        SbLogger.Info(s);
    }

    public void Debug(string? s)
    {
        if (s == null)
            return;

        lock (Messages)
        {
            Messages.Add(new()
            {
                Level = "debug",
                Message = s
            });
        }

        SbLogger.Debug(s);
    }

    public void Warn(string? s)
    {
        if (s == null)
            return;

        lock (Messages)
        {
            Messages.Add(new()
            {
                Level = "warn",
                Message = s
            });
        }

        SbLogger.Warn(s);
    }

    public void Error(string? s)
    {
        if (s == null)
            return;

        lock (Messages)
        {
            Messages.Add(new()
            {
                Level = "error",
                Message = s
            });
        }

        SbLogger.Error(s);
    }

    public void Fatal(string? s)
    {
        if (s == null)
            return;

        lock (Messages)
        {
            Messages.Add(new()
            {
                Level = "fatal",
                Message = s
            });
        }

        SbLogger.Fatal(s);
    }

    public void InfoEx(Exception ex)
    {
        lock (Messages)
        {
            Messages.Add(new()
            {
                Level = "info",
                Message = ex.ToStringDemystified()
            });
        }

        SbLogger.InfoEx(ex);
    }

    public void DebugEx(Exception ex)
    {
        lock (Messages)
        {
            Messages.Add(new()
            {
                Level = "debug",
                Message = ex.ToStringDemystified()
            });
        }

        SbLogger.DebugEx(ex);
    }

    public void WarnEx(Exception ex)
    {
        lock (Messages)
        {
            Messages.Add(new()
            {
                Level = "warn",
                Message = ex.ToStringDemystified()
            });
        }

        SbLogger.WarnEx(ex);
    }

    public void ErrorEx(Exception ex)
    {
        lock (Messages)
        {
            Messages.Add(new()
            {
                Level = "error",
                Message = ex.ToStringDemystified()
            });
        }

        SbLogger.ErrorEx(ex);
    }

    public void FatalEx(Exception ex)
    {
        lock (Messages)
        {
            Messages.Add(new()
            {
                Level = "fatal",
                Message = ex.ToStringDemystified()
            });
        }

        SbLogger.FatalEx(ex);
    }

    public LoggingConfiguration GetErrorConfiguration()
    {
        return SbLogger.GetErrorConfiguration();
    }

    public void SetErrorConfiguration(LoggingConfiguration configuration)
    {
        SbLogger.SetErrorConfiguration(configuration);
    }

    public LoggingConfiguration GetFatalConfiguration()
    {
        return SbLogger.GetFatalConfiguration();
    }

    public void SetFatalConfiguration(LoggingConfiguration configuration)
    {
        SbLogger.SetFatalConfiguration(configuration);
    }

    public LoggingConfiguration GetWarnConfiguration()
    {
        return SbLogger.GetWarnConfiguration();
    }

    public void SetWarnConfiguration(LoggingConfiguration configuration)
    {
        SbLogger.SetWarnConfiguration(configuration);
    }

    public LoggingConfiguration GetInfoConfiguration()
    {
        return SbLogger.GetInfoConfiguration();
    }

    public void SetInfoConfiguration(LoggingConfiguration configuration)
    {
        SbLogger.SetInfoConfiguration(configuration);
    }

    public LoggingConfiguration GetDebugConfiguration()
    {
        return SbLogger.GetDebugConfiguration();
    }

    public void SetDebugConfiguration(LoggingConfiguration configuration)
    {
        SbLogger.SetDebugConfiguration(configuration);
    }

    public ILoggingAddition GetAddition()
    {
        return SbLogger.GetAddition();
    }

    public void SetAddition(ILoggingAddition addition)
    {
        SbLogger.SetAddition(addition);
    }

    public bool LogCallingClass
    {
        get => SbLogger.LogCallingClass;
        set => SbLogger.LogCallingClass = value;
    }
}