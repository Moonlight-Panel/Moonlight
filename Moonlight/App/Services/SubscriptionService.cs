using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Subscriptions;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services;

public class SubscriptionService
{
    private readonly SubscriptionRepository SubscriptionRepository;
    private readonly UserRepository UserRepository;
    private readonly IdentityService IdentityService;
    private readonly ConfigService ConfigService;
    private readonly OneTimeJwtService OneTimeJwtService;

    public SubscriptionService(SubscriptionRepository subscriptionRepository, 
        UserRepository userRepository, 
        IdentityService identityService, 
        ConfigService configService, 
        OneTimeJwtService oneTimeJwtService)
    {
        SubscriptionRepository = subscriptionRepository;
        UserRepository = userRepository;
        IdentityService = identityService;
        ConfigService = configService;
        OneTimeJwtService = oneTimeJwtService;
    }

    public async Task<Subscription?> Get()
    {
        var user = await IdentityService.Get();
        var advancedUser = UserRepository
            .Get()
            .Include(x => x.Subscription)
            .First(x => x.Id == user!.Id);

        if (advancedUser.Subscription == null)
            return null;

        return SubscriptionRepository
            .Get()
            .Include(x => x.Limits)
            .Include("Limits.Image")
            .First(x => x.Id == advancedUser.Subscription.Id);
    }
    public async Task Cancel()
    {
        var user = await IdentityService.Get();
        user!.Subscription = null;
        UserRepository.Update(user!);
    }
    public Task<Subscription[]> GetAvailable()
    {
        return Task.FromResult(
            SubscriptionRepository
                .Get()
                .Include(x => x.Limits)
                .ToArray()
        );
    }
    public Task<string> GenerateBuyUrl(Subscription subscription)
    {
        var url = ConfigService
            .GetSection("Moonlight")
            .GetSection("Payments")
            .GetValue<string>("BaseUrl");

        return Task.FromResult<string>($"{url}/products/{subscription.SellPassId}");
    }
    public Task<string> ProcessGenerate(int subscriptionId)
    {
        var subscription = SubscriptionRepository
            .Get()
            .FirstOrDefault(x => x.Id == subscriptionId);

        if (subscription == null)
            throw new DisplayException("Unknown subscription id");

        var token = OneTimeJwtService.Generate(
            options =>
            {
                options.Add("id", subscription.Id.ToString());
            }
        );

        return Task.FromResult(token);
    }

    public async Task ApplyCode(string code)
    {
        var user = (await IdentityService.Get())!;
        var values = OneTimeJwtService.Validate(code);

        if (values == null)
            throw new DisplayException("Invalid subscription code");

        if (!values.ContainsKey("id"))
            throw new DisplayException("Subscription code is missing the id");

        var id = int.Parse(values["id"]);

        var subscription = SubscriptionRepository
            .Get()
            .FirstOrDefault(x => x.Id == id);

        if (subscription == null)
            throw new DisplayException("The subscription the code is referring does not exist");

        user.Subscription = subscription;
        user.SubscriptionDuration = subscription.Duration;
        user.SubscriptionSince = DateTime.Now;
        
        UserRepository.Update(user);
        
        OneTimeJwtService.Revoke(code);
    }
}