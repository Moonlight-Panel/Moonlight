using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Services;

public class UserAuthService
{
    private readonly ILogger<UserAuthService> Logger;
    private readonly DatabaseRepository<User> UserRepository;
    private readonly AppConfiguration Configuration;

    private const string UserIdClaim = "UserId";
    private const string IssuedAtClaim = "IssuedAt";

    public UserAuthService(
        ILogger<UserAuthService> logger,
        DatabaseRepository<User> userRepository,
        AppConfiguration configuration
    )
    {
        Logger = logger;
        UserRepository = userRepository;
        Configuration = configuration;
    }

    public async Task<bool> Sync(ClaimsPrincipal? principal)
    {
        // Ignore malformed claims principal
        if (principal is not { Identity.IsAuthenticated: true })
            return false;

        // Search for email and username. We need both to create the user model if required.
        // We do a ToLower here because external authentication provider might provide case-sensitive data

        var email = principal.FindFirstValue(ClaimTypes.Email)?.ToLower();
        var username = principal.FindFirstValue(ClaimTypes.Name)?.ToLower();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
        {
            Logger.LogWarning(
                "The authentication scheme {scheme} did not provide claim types: email, name. These are required to sync to user to the database",
                principal.Identity.AuthenticationType
            );

            return false;
        }

        // If you plan to use multiple auth providers it can be a good idea
        // to use an identifier in the user model which consists of the provider and the NameIdentifier
        // instead of the email address. For simplicity, we just use the email as the identifier so multiple auth providers
        // can lead to the same account when the email matches
        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            string[] permissions = [];

            // Yes I know we handle the first user admin thing in the LocalAuth too,
            // but this only works fo the local auth. So if a user uses an external auth scheme
            // like oauth2 discord, the first user admin toggle would do nothing
            if (Configuration.Authentication.FirstUserAdmin)
            {
                var count = await UserRepository
                    .Get()
                    .CountAsync();

                if (count == 0)
                    permissions = ["*"];
            }

            user = await UserRepository.Add(new User()
            {
                Email = email,
                TokenValidTimestamp = DateTimeOffset.UtcNow.AddMinutes(-1),
                Username = username,
                Password = HashHelper.Hash(Formatter.GenerateString(64)),
                Permissions = permissions
            });
        }

        // You can sync other properties here
        if (user.Username != username)
        {
            user.Username = username;
            await UserRepository.Update(user);
        }

        principal.Identities.First().AddClaims([
            new Claim(UserIdClaim, user.Id.ToString()),
            new Claim(IssuedAtClaim, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim("Permissions", string.Join(';', user.Permissions))
        ]);

        return true;
    }

    public async Task<bool> Validate(ClaimsPrincipal? principal)
    {
        // Ignore malformed claims principal
        if (principal is not { Identity.IsAuthenticated: true })
            return false;

        // Validate if the user still exists, and then we want to validate the token issue time
        // against the invalidation time

        var userIdStr = principal.FindFirstValue(UserIdClaim);

        if (!int.TryParse(userIdStr, out var userId))
            return false;

        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        // Token time validation
        var issuedAtStr = principal.FindFirstValue(IssuedAtClaim);

        if (!long.TryParse(issuedAtStr, out var issuedAtUnix))
            return false;

        var issuedAt = DateTimeOffset
            .FromUnixTimeSeconds(issuedAtUnix)
            .ToUniversalTime();

        // If the issued at timestamp is greater than the token validation timestamp
        // everything is fine. If not it means that the token should be invalidated
        // as it is too old

        return issuedAt > user.TokenValidTimestamp;
    }
}