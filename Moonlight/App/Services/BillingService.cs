using System.Globalization;
using Moonlight.App.Database.Entities;
using Moonlight.App.Events;
using Moonlight.App.Exceptions;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Mail;
using Moonlight.App.Services.Sessions;
using Stripe.Checkout;
using Subscription = Moonlight.App.Database.Entities.Subscription;

namespace Moonlight.App.Services;

public class BillingService
{
    private readonly ConfigService ConfigService;
    private readonly SubscriptionService SubscriptionService;
    private readonly Repository<Subscription> SubscriptionRepository;
    private readonly SessionServerService SessionServerService;
    private readonly EventSystem Event;
    private readonly MailService MailService;

    public BillingService(
        ConfigService configService,
        SubscriptionService subscriptionService,
        Repository<Subscription> subscriptionRepository,
        EventSystem eventSystem,
        SessionServerService sessionServerService,
        MailService mailService)
    {
        ConfigService = configService;
        SubscriptionService = subscriptionService;
        SubscriptionRepository = subscriptionRepository;
        Event = eventSystem;
        SessionServerService = sessionServerService;
        MailService = mailService;
    }

    public async Task<string> StartCheckout(User user, Subscription subscription)
    {
        var appUrl = ConfigService.Get().Moonlight.AppUrl;
        var controllerUrl = appUrl + "/api/moonlight/billing";
        
        var options = new SessionCreateOptions()
        {
            LineItems = new()
            {
                new()
                {
                    Price = subscription.StripePriceId,
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = controllerUrl + "/success",
            CancelUrl = controllerUrl + "/cancel",
            AutomaticTax = new SessionAutomaticTaxOptions()
            {
                Enabled = true
            },
            CustomerEmail = user.Email.ToLower(),
            Metadata = new()
            {
                {
                    "productId",
                    subscription.StripeProductId
                }
            }
        };

        var service = new SessionService();

        var session = await service.CreateAsync(options);

        return session.Url;
    }
    public async Task CompleteCheckout(User user)
    {
        var sessionService = new SessionService();

        var sessionsPerUser = await sessionService.ListAsync(new SessionListOptions()
        {
            CustomerDetails = new()
            {
                Email = user.Email
            }
        });

        var latestCompletedSession = sessionsPerUser
            .Where(x => x.Status == "complete")
            .Where(x => x.PaymentStatus == "paid")
            .MaxBy(x => x.Created);

        if (latestCompletedSession == null)
            throw new DisplayException("No completed session found");

        var productId = latestCompletedSession.Metadata["productId"];

        var subscription = SubscriptionRepository
            .Get()
            .FirstOrDefault(x => x.StripeProductId == productId);

        if (subscription == null)
            throw new DisplayException("No subscription for this product found");

        // if (await SubscriptionService.GetActiveSubscription(user) != null)
        // {
        //     return;
        // }

        await SubscriptionService.SetActiveSubscription(user, subscription);

        await MailService.SendMail(user, "checkoutComplete", values =>
        {
            values.Add("SubscriptionName", subscription.Name);
            values.Add("SubscriptionPrice", subscription.Price
                .ToString(CultureInfo.InvariantCulture));
            values.Add("SubscriptionCurrency", subscription.Currency
                .ToString());
            values.Add("SubscriptionDuration", subscription.Duration
                .ToString(CultureInfo.InvariantCulture));
        });
        
        await Event.Emit("billing.completed", user);

        await SessionServerService.ReloadUserSessions(user);
    }
}