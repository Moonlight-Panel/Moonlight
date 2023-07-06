using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Newtonsoft.Json;
using Stripe;
using File = System.IO.File;
using Subscription = Moonlight.App.Database.Entities.Subscription;

namespace Moonlight.App.Services;

public class SubscriptionService
{
    private readonly Repository<Subscription> SubscriptionRepository;
    private readonly Repository<User> UserRepository;

    public SubscriptionService(
        Repository<Subscription> subscriptionRepository,
        Repository<User> userRepository)
    {
        SubscriptionRepository = subscriptionRepository;
        UserRepository = userRepository;
    }

    public async Task<Subscription> Create(string name, string description, Currency currency, double price, int duration)
    {
        var optionsProduct = new ProductCreateOptions
        {
            Name = name,
            Description = description,
            DefaultPriceData = new()
            {
                UnitAmount = (long)(price * 100),
                Currency = currency.ToString().ToLower()
            }
        };

        var productService = new ProductService();
        var product = await productService.CreateAsync(optionsProduct);

        var subscription = new Subscription()
        {
            Name = name,
            Description = description,
            Currency = currency,
            Price = price,
            Duration = duration,
            LimitsJson = "[]",
            StripeProductId = product.Id,
            StripePriceId = product.DefaultPriceId
        };

        return SubscriptionRepository.Add(subscription);
    }
    public async Task Update(Subscription subscription)
    {
        // Create the new price object
        
        var optionsPrice = new PriceCreateOptions
        {
            UnitAmount = (long)(subscription.Price * 100),
            Currency = subscription.Currency.ToString().ToLower(),
            Product = subscription.StripeProductId
        };

        var servicePrice = new PriceService();
        var price = await servicePrice.CreateAsync(optionsPrice);
        
        // Update the product
        
        var productService = new ProductService();
        var product = await productService.UpdateAsync(subscription.StripeProductId, new()
        {
            Name = subscription.Name,
            Description = subscription.Description,
            DefaultPrice = price.Id
        });

        // Disable old price
        await servicePrice.UpdateAsync(subscription.StripePriceId, new()
        {
            Active = false
        });

        // Update the model

        subscription.StripeProductId = product.Id;
        subscription.StripePriceId = product.DefaultPriceId;
        
        SubscriptionRepository.Update(subscription);
    }
    public async Task Delete(Subscription subscription)
    {
        var productService = new ProductService();
        await productService.DeleteAsync(subscription.StripeProductId);
        
        SubscriptionRepository.Delete(subscription);
    }

    public Task UpdateLimits(Subscription subscription, SubscriptionLimit[] limits)
    {
        subscription.LimitsJson = JsonConvert.SerializeObject(limits);
        SubscriptionRepository.Update(subscription);
        
        return Task.CompletedTask;
    }
    public Task<SubscriptionLimit[]> GetLimits(Subscription subscription)
    {
        var limits = 
            JsonConvert.DeserializeObject<SubscriptionLimit[]>(subscription.LimitsJson) ?? Array.Empty<SubscriptionLimit>();
        return Task.FromResult(limits);
    }

    public async Task<Subscription?> GetActiveSubscription(User u)
    {
        var user = await EnsureData(u);

        if (user.CurrentSubscription != null)
        {
            if (user.SubscriptionExpires < DateTime.UtcNow)
            {
                user.CurrentSubscription = null;
                UserRepository.Update(user);
            }
        }
        
        return user.CurrentSubscription;
    }
    public async Task CancelSubscription(User u)
    {
        var user = await EnsureData(u);

        user.CurrentSubscription = null;
        UserRepository.Update(user);
    }
    public async Task SetActiveSubscription(User u, Subscription subscription)
    {
        var user = await EnsureData(u);
        
        user.SubscriptionSince = DateTime.UtcNow;
        user.SubscriptionExpires = DateTime.UtcNow.AddDays(subscription.Duration);
        user.CurrentSubscription = subscription;
        
        UserRepository.Update(user);
    }
    
    public async Task<SubscriptionLimit[]> GetDefaultLimits()
    {
        var defaultSubscriptionJson = "[]";
        var path = PathBuilder.File("storage", "configs", "default_subscription.json");

        if (File.Exists(path))
        {
            defaultSubscriptionJson =
                await File.ReadAllTextAsync(path);
        }

        return JsonConvert.DeserializeObject<SubscriptionLimit[]>(defaultSubscriptionJson)
               ?? Array.Empty<SubscriptionLimit>();
    }
    public async Task<SubscriptionLimit> GetLimit(User u, string identifier)
    {
        var subscription = await GetActiveSubscription(u);
        var defaultLimits = await GetDefaultLimits();

        if (subscription != null) // User has a active subscriptions
        {
            var subscriptionLimits = await GetLimits(subscription);

            var subscriptionLimit = subscriptionLimits
                .FirstOrDefault(x => x.Identifier == identifier);

            if (subscriptionLimit != null) // Found subscription limit for the user's subscription
                return subscriptionLimit;
        } // If were are here, the user's subscription has no limit for this identifier, so we fallback to default
        
        var defaultSubscriptionLimit = defaultLimits
            .FirstOrDefault(x => x.Identifier == identifier);

        if (defaultSubscriptionLimit != null)
            return defaultSubscriptionLimit; // Default subscription limit found

        return new() // No default subscription limit found
        {
            Identifier = identifier,
            Amount = 0
        };
    }

    private Task<User> EnsureData(User u)
    {
        var user = UserRepository
            .Get()
            .Include(x => x.CurrentSubscription)
            .First(x => x.Id == u.Id);

        return Task.FromResult(user);
    }
}