using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Services;
using Moonlight.App.Services.Sessions;
using Stripe;
using Stripe.Checkout;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/billing")]
public class BillingController : Controller
{
    private readonly IdentityService IdentityService;
    private readonly BillingService BillingService;

    public BillingController(
        IdentityService identityService,
        BillingService billingService)
    {
        IdentityService = identityService;
        BillingService = billingService;
    }

    [HttpGet("cancel")]
    public async Task<ActionResult> Cancel()
    {
        var user = IdentityService.User;

        if (user == null)
            return Redirect("/login");
        
        return Redirect("/profile/subscriptions/close");
    }
    
    [HttpGet("success")]
    public async Task<ActionResult> Success()
    {
        var user = IdentityService.User;

        if (user == null)
            return Redirect("/login");

        await BillingService.CompleteCheckout(user);
        
        return Redirect("/profile/subscriptions/close");
    }
}