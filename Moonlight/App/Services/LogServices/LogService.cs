using Logging.Net;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;

namespace Moonlight.App.Services.LogServices;

public class LogService
{
    public LogService()
    {
        Task.Run(ClearLog);
    }

    private async Task ClearLog()
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromMinutes(15));

            if (GetMessages().Length > 500)
            {
                if (Logger.UsedLogger is CacheLogger cacheLogger)
                {
                    cacheLogger.Clear(250); //TODO: config
                }
                else
                {
                    Logger.Warn("Log service cannot access cache. Is Logging.Net using CacheLogger?");
                }
            }
        }
    }
    
    public LogEntry[] GetMessages()
    {
        if (Logger.UsedLogger is CacheLogger cacheLogger)
        {
            return cacheLogger.GetMessages();
        }

        Logger.Warn("Log service cannot access cache. Is Logging.Net using CacheLogger?");
        
        return Array.Empty<LogEntry>();
    }
}