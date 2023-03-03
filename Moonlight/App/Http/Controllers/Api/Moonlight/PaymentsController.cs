using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Services;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/payments")]
public class PaymentsController : Controller
{
    private readonly ConfigService ConfigService;
    private readonly SubscriptionService SubscriptionService;

    public PaymentsController(ConfigService configService, SubscriptionService subscriptionService)
    {
        ConfigService = configService;
        SubscriptionService = subscriptionService;
    }

    [HttpGet("generate")]
    public async Task<ActionResult> GenerateGet([FromQuery] string key, [FromQuery] int subscriptionId)
    {
        var validKey = ConfigService
            .GetSection("Moonlight")
            .GetSection("Payments")
            .GetValue<string>("Key");

        if (key != validKey)
            return StatusCode(403);

        var token = await SubscriptionService.ProcessGenerate(subscriptionId);

        return Ok(token);
    }
    
    [HttpPost("generate")]
    public async Task<ActionResult> GeneratePost([FromQuery] string key, [FromQuery] int subscriptionId)
    {
        var validKey = ConfigService
            .GetSection("Moonlight")
            .GetSection("Payments")
            .GetValue<string>("Key");

        if (key != validKey)
            return StatusCode(403);

        var token = await SubscriptionService.ProcessGenerate(subscriptionId);

        return Ok(token);
    }
}