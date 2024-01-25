using Moonlight.Core.Helpers;

namespace Moonlight.Core.Services.Utils;

public class ConnectionService
{
    private readonly IHttpContextAccessor ContextAccessor;
    private readonly ConfigService ConfigService;

    public ConnectionService(IHttpContextAccessor contextAccessor, ConfigService configService)
    {
        ContextAccessor = contextAccessor;
        ConfigService = configService;
    }

    public Task<string> GetIpAddress()
    {
        if (ContextAccessor.HttpContext == null)
            return Task.FromResult("N/A (Missing http context)");

        var request = ContextAccessor.HttpContext.Request;
        
        if (request.Headers.ContainsKey("X-Real-IP"))
        {
            if(ConfigService.Get().Security.EnableReverseProxyMode)
                return Task.FromResult(request.Headers["X-Real-IP"].ToString());
            
            Logger.Warn($"Detected an ip mask attempt by using a fake X-Real-IP header. Fake IP: {request.Headers["X-Real-IP"]}. Real IP: {ContextAccessor.HttpContext.Connection.RemoteIpAddress}");
            return Task.FromResult(ContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A (Remote IP missing)");
        }
        
        return Task.FromResult(ContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A (Remote IP missing)");
    }
}