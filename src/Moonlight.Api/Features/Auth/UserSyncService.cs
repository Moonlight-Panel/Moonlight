using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Moonlight.Api.Infrastructure.Database;
using Moonlight.ApiSdk.Features.Users;

namespace Moonlight.Api.Features.Auth;

public class UserSyncService
{
    private readonly DataContext _dataContext;
    private readonly ILogger<UserSyncService> _logger;
    private readonly HybridCache _cache;

    private const string CacheKeyTemplate = $"Moonlight.{nameof(UserSyncService)}.{{0}}";

    public UserSyncService(
        DataContext dataContext,
        ILogger<UserSyncService> logger,
        HybridCache cache
    )
    {
        _dataContext = dataContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task SyncAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var username = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(username))
        {
            _logger.LogDebug("Received no username with request. Cannot sync user");
            return;
        }

        var cacheKey = string.Format(CacheKeyTemplate, username);

        _ = await _cache.GetOrCreateAsync(
            cacheKey,
            (ct) => SyncUserAsync(username, principal, ct),
            new HybridCacheEntryOptions()
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            },
            cancellationToken: cancellationToken
        );
    }

    private async ValueTask<bool> SyncUserAsync(string username, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await _dataContext
            .Users
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken: cancellationToken);

        if (user == null)
        {
            // Create missing user
            user = new User()
            {
                DisplayName = principal.FindFirstValue(ClaimTypes.Name) ?? username,
                Email = principal.FindFirstValue(ClaimTypes.Email),
                IsDeleted = false,
                Username = username.ToLower(),
                AllowLocalAuth = false,
                PasswordHash = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _dataContext.Users.Add(user);
            
            _logger.LogTrace("Syncing new user {username}", username);
        }
        else
        {
            // Sync existing user
            user.Email = principal.FindFirstValue(ClaimTypes.Email) ?? user.Email;
            user.DisplayName = principal.FindFirstValue(ClaimTypes.Name) ?? user.DisplayName;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            
            _dataContext.Users.Update(user);
            
            _logger.LogTrace("Syncing existing user {username}", username);
        }

        await _dataContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}