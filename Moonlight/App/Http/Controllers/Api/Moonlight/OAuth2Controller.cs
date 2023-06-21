using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Helpers;
using Moonlight.App.Services;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/oauth2")]
public class OAuth2Controller : Controller
{
    private readonly UserService UserService;
    private readonly OAuth2Service OAuth2Service;
    private readonly DateTimeService DateTimeService;
    private readonly IdentityService IdentityService;

    public OAuth2Controller(
        UserService userService,
        OAuth2Service oAuth2Service,
        DateTimeService dateTimeService,
        IdentityService identityService)
    {
        UserService = userService;
        OAuth2Service = oAuth2Service;
        DateTimeService = dateTimeService;
        IdentityService = identityService;
    }

    [HttpGet("{id}/start")]
    public async Task<ActionResult> Start([FromRoute] string id)
    {
        try
        {
            if (OAuth2Service.Providers.ContainsKey(id))
            {
                return Redirect(await OAuth2Service.GetUrl(id));
            }
            
            Logger.Warn($"Someone tried to start an oauth2 flow using the id '{id}' which is not registered");

            return Redirect("/");
        }
        catch (Exception e)
        {
            Logger.Warn($"Error starting oauth2 flow for id: {id}");
            Logger.Warn(e);
            
            return Redirect("/");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Hande([FromRoute] string id, [FromQuery] string code)
    {
        try
        {
            var currentUser = await IdentityService.Get();

            if (currentUser != null)
            {
                if (await OAuth2Service.CanBeLinked(id))
                {
                    await OAuth2Service.LinkToUser(id, currentUser, code);
                    
                    return Redirect("/profile");
                }
            }
            
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