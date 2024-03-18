using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Models.Enums;

namespace Moonlight.Core.Services;

[Scoped]
public class IdentityService : IDisposable
{
    public User? CurrentUserNullable { get; private set; }
    public User CurrentUser => CurrentUserNullable!;
    public bool IsLoggedIn => CurrentUserNullable != null;
    public SmartEventHandler OnAuthenticationStateChanged { get; set; } = new();

    private string Token = "";

    private readonly JwtService<CoreJwtType> JwtService;
    private readonly ConfigService<CoreConfiguration> ConfigService;
    private readonly Repository<User> UserRepository;

    public IdentityService(
        JwtService<CoreJwtType> jwtService,
        ConfigService<CoreConfiguration> configService,
        Repository<User> userRepository)
    {
        JwtService = jwtService;
        ConfigService = configService;
        UserRepository = userRepository;
    }

    public async Task<string> Authenticate(User user)
    {
        var duration = TimeSpan.FromDays(ConfigService.Get().Authentication.TokenDuration);

        var token = await JwtService.Create(data =>
        {
            data.Add("UserId", user.Id.ToString());
            data.Add("IssuedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }, CoreJwtType.Login, duration);

        await Authenticate(token);

        return token;
    }

    public async Task Authenticate(string token)
    {
        Token = token;

        await Authenticate();
    }
    
    public async Task Authenticate(bool forceStateChange = false) // Can be used for authentication of the token as well
    {
        var lastUserId = CurrentUserNullable?.Id ?? -1;

        await ProcessToken();
        
        var currentUserId = CurrentUserNullable?.Id ?? -1;

        if (lastUserId != currentUserId || forceStateChange)
            await OnAuthenticationStateChanged.Invoke();
    }

    private async Task ProcessToken()
    {
        CurrentUserNullable = null;
        
        // Check jwt signature
        if (!await JwtService.Validate(Token, CoreJwtType.Login))
            return;

        var data = await JwtService.Decode(Token);
        
        // Check for missing content
        if(!data.ContainsKey("UserId") || !data.ContainsKey("IssuedAt"))
            return;

        // Load user
        var userId = int.Parse(data["UserId"]);

        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Id == userId);
        
        // Check if user was found
        if(user == null)
            return;
        
        // Check token valid time
        var issuedAt = long.Parse(data["IssuedAt"]);
        var issuedAtDateTime = DateTimeOffset.FromUnixTimeSeconds(issuedAt).DateTime;

        // If the valid time is newer then when the token was issued, the token is not longer valid
        if (user.TokenValidTimestamp > issuedAtDateTime)
            return;

        CurrentUserNullable = user;
    }

    public Task<bool> HasFlag(string flag)
    {
        if (!IsLoggedIn)
            return Task.FromResult(false);
        
        var flags = CurrentUser.Flags.Split(";");

        return Task.FromResult(flags.Contains(flag));
    }

    public Task SetFlag(string flag, bool toggle)
    {
        if (!IsLoggedIn)
            throw new DisplayException("Unable to set flag while not logged in");
        
        var flags = CurrentUser.Flags.Split(";").ToList();

        if (toggle)
        {
            if(!flags.Contains(flag))
                flags.Add(flag);
        }
        else
        {
            if (flags.Contains(flag))
                flags.Remove(flag);
        }

        CurrentUser.Flags = string.Join(';', flags);
        UserRepository.Update(CurrentUser);
        
        return Task.CompletedTask;
    }

    public async void Dispose()
    {
        await OnAuthenticationStateChanged.ClearSubscribers();
    }
}