using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Events;
using Moonlight.App.Services;

namespace Moonlight.App.Http.Controllers.WellKnown;

[ApiController]
[Route(".well-known/acme-challenge")]
public class AcmeController : Controller
{
    private readonly LetsEncryptService LetsEncryptService;
    private readonly EventSystem Event;

    public AcmeController(LetsEncryptService letsEncryptService, EventSystem eventSystem)
    {
        LetsEncryptService = letsEncryptService;
        Event = eventSystem;
    }

    [HttpGet("{token}")]
    public async Task<ActionResult> Get([FromRoute] string token)
    {
        if (string.IsNullOrEmpty(LetsEncryptService.HttpChallenge) || string.IsNullOrEmpty(LetsEncryptService.HttpChallengeToken))
            return Problem();

        if (string.IsNullOrEmpty(token) || LetsEncryptService.HttpChallengeToken != token)
            return Problem();

        await Event.Emit("letsEncrypt.challengeFetched");

        return Ok(LetsEncryptService.HttpChallenge);
    }
}