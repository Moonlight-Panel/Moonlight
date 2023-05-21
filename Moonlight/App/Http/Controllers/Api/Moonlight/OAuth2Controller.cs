using Logging.Net;
using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Services;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/oauth2")]
public class OAuth2Controller : Controller
{
    private readonly UserService UserService;
    private readonly OAuth2Service OAuth2Service;
    private readonly DateTimeService DateTimeService;

    public OAuth2Controller(UserService userService, OAuth2Service oAuth2Service, DateTimeService dateTimeService)
    {
        UserService = userService;
        OAuth2Service = oAuth2Service;
        DateTimeService = dateTimeService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Hande([FromRoute] string id, [FromQuery] string code)
    {
        try
        {
            var user = await OAuth2Service.HandleCode(id, code);

            Response.Cookies.Append("token", await UserService.GenerateToken(user), new()
            {
                Expires = new DateTimeOffset(DateTimeService.GetCurrent().AddDays(10))
            });

            return Redirect("/");
        }
        catch (Exception e)
        {
            Logger.Warn("An unexpected error occured while handling oauth2");
            Logger.Warn(e.Message);

            return Redirect("/login");
        }
    }
}