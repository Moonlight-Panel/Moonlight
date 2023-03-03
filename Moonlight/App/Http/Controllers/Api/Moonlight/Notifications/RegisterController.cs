using GravatarSharp;
using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Models.Notifications;
using Moonlight.App.Repositories;
using Moonlight.App.Services;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Http.Controllers.Api.Moonlight.Notifications;

[ApiController]
[Route("api/moonlight/notifications/register")]
public class RegisterController : Controller
{
    private readonly IdentityService IdentityService;
    private readonly NotificationRepository NotificationRepository;
    private readonly OneTimeJwtService OneTimeJwtService;

    public RegisterController(IdentityService identityService, NotificationRepository notificationRepository, OneTimeJwtService oneTimeJwtService)
    {
        IdentityService = identityService;
        NotificationRepository = notificationRepository;
        OneTimeJwtService = oneTimeJwtService;
    }

    [HttpGet]
    public async Task<ActionResult<TokenRegister>> Register()
    {
        var user = await IdentityService.Get();

        if (user == null)
            return NotFound();
        
        try
        {
            var id = NotificationRepository.RegisterNewDevice(user);

            return new TokenRegister()
            {
                Token = OneTimeJwtService.Generate((dict) =>
                {
                    dict["clientId"] = id.ToString();
                }, TimeSpan.FromDays(31))
            };
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }
}