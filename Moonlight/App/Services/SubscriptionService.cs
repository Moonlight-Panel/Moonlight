using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Sessions;
using Newtonsoft.Json;

namespace Moonlight.App.Services;

public class SubscriptionService
{
    private readonly SubscriptionRepository SubscriptionRepository;
    private readonly OneTimeJwtService OneTimeJwtService;
    private readonly IdentityService IdentityService;
    private readonly UserRepository UserRepository;

    public SubscriptionService(
        SubscriptionRepository subscriptionRepository,
        OneTimeJwtService oneTimeJwtService,
        IdentityService identityService,
        UserRepository userRepository
    )
    {
        SubscriptionRepository = subscriptionRepository;
        OneTimeJwtService = oneTimeJwtService;
        IdentityService = identityService;
        UserRepository = userRepository;
    }

    public async Task<Subscription?> GetCurrent()
    {
        var user = await GetCurrentUser();

        if (user == null || user.CurrentSubscription == null)
            return null;

        var subscriptionEnd = user.SubscriptionSince.ToUniversalTime().AddDays(user.SubscriptionDuration);

        if (subscriptionEnd > DateTime.UtcNow)
        {
            return user.CurrentSubscription;
        }

        return null;
    }

    public async Task ApplyCode(string code)
    {
        var data = await OneTimeJwtService.Validate(code);

        if (data == null)
            throw new DisplayException("Invalid or expired subscription code");

        var id = int.Parse(data["subscription"]);
        var duration = int.Parse(data["duration"]);

        var subscription = SubscriptionRepository
            .Get()
            .FirstOrDefault(x => x.Id == id);

        if (subscription == null)
            throw new DisplayException("The subscription the code is associated with does not exist");

        var user = await GetCurrentUser();

        if (user == null)
            throw new DisplayException("Unable to determine current user");

        user.CurrentSubscription = subscription;
        user.SubscriptionDuration = duration;
        user.SubscriptionSince = DateTime.UtcNow;
        
        UserRepository.Update(user);

        await OneTimeJwtService.Revoke(code);
    }

    public async Task Cancel()
    {
        if (await GetCurrent() != null)
        {
            var user = await GetCurrentUser();

            user.CurrentSubscription = null;
            
            UserRepository.Update(user);
        }
    }

    public async Task<SubscriptionLimit> GetLimit(string identifier) // Cache, optimize sql code
    {
        var subscription = await GetCurrent();
        var defaultLimits = await GetDefaultLimits();

        if (subscription == null)
        {
            // If the default subscription limit with identifier is found, return it. if not, return empty
            return defaultLimits.FirstOrDefault(x => x.Identifier == identifier) ?? new()
            {
                Identifier = identifier,
                Amount = 0
            };
        }

        var subscriptionLimits = 
            JsonConvert.DeserializeObject<SubscriptionLimit[]>(subscription.LimitsJson) 
            ?? Array.Empty<SubscriptionLimit>();

        var foundLimit = subscriptionLimits.FirstOrDefault(x => x.Identifier == identifier);

        if (foundLimit != null)
            return foundLimit;
        
        // If the default subscription limit with identifier is found, return it. if not, return empty
        return defaultLimits.FirstOrDefault(x => x.Identifier == identifier) ?? new()
        {
            Identifier = identifier,
            Amount = 0
        };
    }

    private async Task<User?> GetCurrentUser()
    {
        var user = await IdentityService.Get();

        if (user == null)
            return null;

        var userWithData = UserRepository
            .Get()
            .Include(x => x.CurrentSubscription)
            .First(x => x.Id == user.Id);

        return userWithData;
    }

    private async Task<SubscriptionLimit[]> GetDefaultLimits() // Add cache and reload option
    {
        var defaultSubscriptionJson = "[]";

        if (File.Exists(PathBuilder.File("storage", "configs", "default_subscription.json")))
        {
            defaultSubscriptionJson =
                await File.ReadAllTextAsync(PathBuilder.File("storage", "configs", "default_subscription.json"));
        }

        return JsonConvert.DeserializeObject<SubscriptionLimit[]>(defaultSubscriptionJson) ?? Array.Empty<SubscriptionLimit>();
    }
}