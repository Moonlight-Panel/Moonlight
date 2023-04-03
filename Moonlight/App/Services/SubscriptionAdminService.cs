using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Newtonsoft.Json;

namespace Moonlight.App.Services;

public class SubscriptionAdminService
{
    private readonly SubscriptionRepository SubscriptionRepository;
    private readonly OneTimeJwtService OneTimeJwtService;

    public SubscriptionAdminService(OneTimeJwtService oneTimeJwtService, SubscriptionRepository subscriptionRepository)
    {
        OneTimeJwtService = oneTimeJwtService;
        SubscriptionRepository = subscriptionRepository;
    }

    public Task<SubscriptionLimit[]> GetLimits(Subscription subscription)
    {
        return Task.FromResult(
            JsonConvert.DeserializeObject<SubscriptionLimit[]>(subscription.LimitsJson) 
            ?? Array.Empty<SubscriptionLimit>()
        );
    }

    public Task SaveLimits(Subscription subscription, SubscriptionLimit[] limits)
    {
        subscription.LimitsJson = JsonConvert.SerializeObject(limits);
        SubscriptionRepository.Update(subscription);
        
        return Task.CompletedTask;
    }

    public Task<string> GenerateCode(Subscription subscription, int duration)
    {
        return Task.FromResult(
            OneTimeJwtService.Generate(data =>
            {
                data.Add("subscription", subscription.Id.ToString());
                data.Add("duration", duration.ToString());
            }, TimeSpan.FromDays(10324))
        );
    }
}