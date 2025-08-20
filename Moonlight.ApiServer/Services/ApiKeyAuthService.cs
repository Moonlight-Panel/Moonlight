using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MoonCore.Extended.Abstractions;
using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Services;

public class ApiKeyAuthService
{
    private readonly DatabaseRepository<ApiKey> ApiKeyRepository;

    public ApiKeyAuthService(DatabaseRepository<ApiKey> apiKeyRepository)
    {
        ApiKeyRepository = apiKeyRepository;
    }

    public async Task<bool> Validate(ClaimsPrincipal? principal)
    {
        // Ignore malformed claims principal
        if (principal is not { Identity.IsAuthenticated: true })
            return false;

        var apiKeyIdStr = principal.FindFirstValue("ApiKeyId");

        if (!int.TryParse(apiKeyIdStr, out var apiKeyId))
            return false;

        return await ApiKeyRepository
            .Get()
            .AnyAsync(x => x.Id == apiKeyId);
    }
}