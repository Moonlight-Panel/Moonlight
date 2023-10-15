using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Abstractions;
using Moonlight.App.Models.Enums;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Utils;
using OtpNet;

namespace Moonlight.App.Services;

// This service allows you to reauthenticate, login and force login
// It does also contain the permission system accessor for the current user
public class IdentityService
{
    private readonly Repository<User> UserRepository;
    private readonly JwtService JwtService;

    private string Token;

    public User? CurrentUserNullable { get; private set; }
    public User CurrentUser => CurrentUserNullable!;
    public bool IsSignedIn => CurrentUserNullable != null;
    public FlagStorage Flags { get; private set; } = new("");
    public PermissionStorage Permissions { get; private set; } = new(-1);
    public EventHandler OnAuthenticationStateChanged { get; set; }

    public IdentityService(Repository<User> userRepository,
        JwtService jwtService)
    {
        UserRepository = userRepository;
        JwtService = jwtService;
    }

    // Authentication

    public async Task Authenticate() // Reauthenticate
    {
        // Save the last id (or -1 if not set) so we can track a change
        var lastUserId = CurrentUserNullable == null ? -1 : CurrentUserNullable.Id;

        // Reset
        CurrentUserNullable = null;

        await ValidateToken();

        // Get current user id to compare against the last one
        var currentUserId = CurrentUserNullable == null ? -1 : CurrentUserNullable.Id;

        if (lastUserId != currentUserId) // State changed, lets notify all event listeners
            OnAuthenticationStateChanged?.Invoke(this, null!);
    }

    private async Task ValidateToken() // Read and validate token
    {
        if (string.IsNullOrEmpty(Token))
            return;

        if (!await JwtService.Validate(Token))
            return;

        var data = await JwtService.Decode(Token);

        if (!data.ContainsKey("userId"))
            return;

        var userId = int.Parse(data["userId"]);

        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Id == userId);

        if (user == null)
            return;

        if (!data.ContainsKey("issuedAt"))
            return;

        var issuedAt = long.Parse(data["issuedAt"]);
        var issuedAtDateTime = DateTimeOffset.FromUnixTimeSeconds(issuedAt).DateTime;

        // If the valid time is newer then when the token was issued, the token is not longer valid
        if (user.TokenValidTimestamp > issuedAtDateTime)
            return;

        CurrentUserNullable = user;

        if (CurrentUserNullable == null) // If the current user is null, stop loading additional data
            return;

        Flags = new(CurrentUser.Flags);
        Permissions = new(CurrentUser.Permissions);
    }

    public async Task<string> Login(string email, string password, string? code = null)
    {
        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Email == email);

        if (user == null)
            throw new DisplayException("A user with these credential combination was not found");

        if (!HashHelper.Verify(password, user.Password))
            throw new DisplayException("A user with these credential combination was not found");

        var flags = new FlagStorage(user.Flags); // Construct FlagStorage to check for 2fa

        if (!flags[UserFlag.TotpEnabled]) // No 2fa found on this user so were done here
            return await GenerateToken(user);

        // If we reach this point, 2fa is enabled so we need to continue validating

        if (string.IsNullOrEmpty(code)) // This will show an additional 2fa login field
            throw new ArgumentNullException(nameof(code), "2FA code missing");

        if (user.TotpKey == null) // Hopefully we will never fulfill this check ;)
            throw new DisplayException("2FA key is missing. Please contact the support to fix your account");

        // Calculate server side code
        var totp = new Totp(Base32Encoding.ToBytes(user.TotpKey));
        var codeServerSide = totp.ComputeTotp();

        if (codeServerSide == code)
            return await GenerateToken(user);

        throw new DisplayException("Invalid 2fa code entered");
    }

    public async Task<string> GenerateToken(User user)
    {
        var token = await JwtService.Create(data =>
        {
            data.Add("userId", user.Id.ToString());
            data.Add("issuedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }, TimeSpan.FromDays(10));

        return token;
    }

    public Task SaveFlags()
    {
        // Prevent saving flags for an empty user
        if (!IsSignedIn)
            return Task.CompletedTask;

        // Save the new flag string
        CurrentUser.Flags = Flags.RawFlagString;
        UserRepository.Update(CurrentUser);

        return Task.CompletedTask;
    }

    // Helpers and overloads
    public async Task
        Authenticate(HttpRequest request) // Overload for api controllers to authenticate a user like the normal panel
    {
        if (request.Cookies.ContainsKey("token"))
        {
            var token = request.Cookies["token"];
            await Authenticate(token!);
        }
    }

    public async Task Authenticate(string token) // Overload to set token and reauth
    {
        Token = token;
        await Authenticate();
    }
}