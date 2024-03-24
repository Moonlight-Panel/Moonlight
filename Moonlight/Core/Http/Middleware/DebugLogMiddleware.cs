using MoonCore.Helpers;

namespace Moonlight.Core.Http.Middleware;

public class DebugLogMiddleware
{
    private RequestDelegate Next;

    public DebugLogMiddleware(RequestDelegate next)
    {
        Next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        Logger.Debug($"[{context.Request.Method.ToUpper()}] {context.Request.Path}");
        
        await Next(context);
    }
}