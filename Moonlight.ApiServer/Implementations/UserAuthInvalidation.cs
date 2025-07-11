using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.JwtInvalidation;
using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Implementations;

public class UserAuthInvalidation : IJwtInvalidateHandler
{
    private readonly DatabaseRepository<User> UserRepository;
    private readonly DatabaseRepository<ApiKey> ApiKeyRepository;

    public UserAuthInvalidation(
        DatabaseRepository<User> userRepository,
        DatabaseRepository<ApiKey> apiKeyRepository
    )
    {
        UserRepository = userRepository;
        ApiKeyRepository = apiKeyRepository;
    }

    public async Task<bool> Handle(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirstValue("userId");

        if (!string.IsNullOrEmpty(userIdClaim))
        {
            var userId = int.Parse(userIdClaim);

            var user = await UserRepository
                .Get()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return true; // User is deleted, invalidate session
            
            var iatStr = principal.FindFirstValue("iat")!;
            var iat = DateTimeOffset.FromUnixTimeSeconds(long.Parse(iatStr));

            // If the token has been issued before the token valid time, its expired, and we want to invalidate it
            return user.TokenValidTimestamp > iat;
        }

        var apiKeyIdClaim = principal.FindFirstValue("apiKeyId");

        if (!string.IsNullOrEmpty(apiKeyIdClaim))
        {
            var apiKeyId = int.Parse(apiKeyIdClaim);

            var apiKey = await ApiKeyRepository
                .Get()
                .FirstOrDefaultAsync(x => x.Id == apiKeyId);

            // If the api key exists, we don't want to invalidate the request.
            // If it doesn't exist we want to invalidate the request
            return apiKey == null;
        }

        return true;
    }
}