using MoonCore.Helpers;

namespace Moonlight.Core.Http.Middleware;

public class DebugLogMiddleware
{
    private readonly ILogger<DebugLogMiddleware> Logger;
    private RequestDelegate Next;

    public DebugLogMiddleware(RequestDelegate next, ILogger<DebugLogMiddleware> logger)
    {
        Next = next;
        Logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        Logger.LogDebug("[{method}] {path}", context.Request.Method.ToUpper(), context.Request.Path);
        
        await Next(context);
    }
}