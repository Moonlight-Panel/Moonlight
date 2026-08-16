using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Moonlight.Frontend.Features.Auth;

public class ApiTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken
    )
    {
        if (_httpContextAccessor.HttpContext is null)
            throw new Exception("HttpContext not available");

        var accessToken = await _httpContextAccessor
            .HttpContext
            .GetTokenAsync("access_token");
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        return await base.SendAsync(request, cancellationToken);
    }
}