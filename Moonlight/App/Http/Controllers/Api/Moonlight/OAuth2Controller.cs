using Logging.Net;
using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Repositories;
using Moonlight.App.Services;
using Moonlight.App.Services.OAuth2;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/oauth2")]
public class OAuth2Controller : Controller
{
    private readonly GoogleOAuth2Service GoogleOAuth2Service;
    private readonly DiscordOAuth2Service DiscordOAuth2Service;
    private readonly UserRepository UserRepository;
    private readonly UserService UserService;

    public OAuth2Controller(
        GoogleOAuth2Service googleOAuth2Service, 
        UserRepository userRepository, 
        UserService userService,
        DiscordOAuth2Service discordOAuth2Service)
    {
        GoogleOAuth2Service = googleOAuth2Service;
        UserRepository = userRepository;
        UserService = userService;
        DiscordOAuth2Service = discordOAuth2Service;
    }

    [HttpGet("google")]
    public async Task<ActionResult> Google([FromQuery] string code)
    {
        try
        {
            var userData = await GoogleOAuth2Service.HandleCode(code);

            if (userData == null)
                return Redirect("/login");

            try
            {
                var user = UserRepository.Get().FirstOrDefault(x => x.Email == userData.Email);

                string token;
                
                if (user == null)
                {
                    token = await UserService.Register(
                        userData.Email,
                        StringHelper.GenerateString(32),
                        userData.FirstName,
                        userData.LastName
                    );
                }
                else
                {
                    token = await UserService.GenerateToken(user);
                }
                
                Response.Cookies.Append("token", token, new ()
                {
                    Expires = new DateTimeOffset(DateTime.UtcNow.AddDays(10))
                });

                return Redirect("/");
            }
            catch (Exception e)
            {
                Logger.Warn(e.Message);
                return Redirect("/login");
            }
        }
        catch (Exception e)
        {
            Logger.Warn(e.Message);
            return BadRequest();
        }
    }

    [HttpGet("discord")]
    public async Task<ActionResult> Discord([FromQuery] string code)
    {
        try
        {
            var userData = await DiscordOAuth2Service.HandleCode(code);

            if (userData == null)
                return Redirect("/login");

            try
            {
                var user = UserRepository.Get().FirstOrDefault(x => x.Email == userData.Email);

                string token;
                
                if (user == null)
                {
                    token = await UserService.Register(
                        userData.Email,
                        StringHelper.GenerateString(32),
                        userData.FirstName,
                        userData.LastName
                    );
                }
                else
                {
                    token = await UserService.GenerateToken(user);
                }
                
                Response.Cookies.Append("token", token, new ()
                {
                    Expires = new DateTimeOffset(DateTime.UtcNow.AddDays(10))
                });

                return Redirect("/");
            }
            catch (Exception e)
            {
                Logger.Warn(e.Message);
                return Redirect("/login");
            }
        }
        catch (Exception e)
        {
            Logger.Warn(e.Message);
            return BadRequest();
        }
    }
}