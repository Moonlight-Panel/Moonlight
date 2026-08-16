namespace Moonlight.Api.Features.Auth;

public class SyncMiddleware
{
    private readonly RequestDelegate _next;

    public SyncMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated ?? false)
        {
            var userSyncService = httpContext.RequestServices.GetRequiredService<UserSyncService>();
            await userSyncService.SyncAsync(httpContext.User, httpContext.RequestAborted);
        }
        
        await _next(httpContext);
    }
}