using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Extended.OAuth2.ApiServer;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.Attributes;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers.Authentication;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Requests.Auth;
using Moonlight.Shared.Http.Responses.Auth;

namespace Moonlight.ApiServer.Http.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : Controller
{
    private readonly OAuth2Service OAuth2Service;
    private readonly TokenHelper TokenHelper;
    private readonly ConfigService<AppConfiguration> ConfigService;
    private readonly DatabaseRepository<User> UserRepository;

    public AuthController(OAuth2Service oAuth2Service, TokenHelper tokenHelper, DatabaseRepository<User> userRepository, ConfigService<AppConfiguration> configService)
    {
        OAuth2Service = oAuth2Service;
        TokenHelper = tokenHelper;
        UserRepository = userRepository;
        ConfigService = configService;
    }

    [HttpGet("start")]
    public async Task<AuthStartResponse> Start()
    {
        var data = await OAuth2Service.StartAuthorizing();

        return Mapper.Map<AuthStartResponse>(data);
    }

    [HttpPost("refresh")]
    public async Task Refresh([FromBody] RefreshRequest request)
    {
        var authConfig = ConfigService.Get().Authentication;
        
        var tokenPair = await TokenHelper.RefreshPair(
            request.RefreshToken,
            authConfig.MlAccessSecret,
            authConfig.MlRefreshSecret,
            (refreshTokenData, newTokenData) =>
            {
                if (!refreshTokenData.TryGetValue("userId", out var userIdStr) || !int.TryParse(userIdStr, out var userId))
                    return false;
                
                var user = UserRepository.Get().FirstOrDefault(x => x.Id == userId);

                if (user == null)
                    return false;
                
                //TODO: External check
                
                newTokenData.Add("userId", user.Id.ToString());
                return true;
            }
        );

        if (!tokenPair.HasValue)
            throw new HttpApiException("Unable to refresh token", 401);
        
        Response.Cookies.Append("ml-access", tokenPair.Value.AccessToken);
        Response.Cookies.Append("ml-refresh", tokenPair.Value.RefreshToken);
        Response.Cookies.Append("ml-timestamp", DateTimeOffset.UtcNow.AddSeconds(3600).ToUnixTimeSeconds().ToString());
    }

    [HttpGet("handle")]
    public async Task Handle([FromQuery(Name = "code")] string code)
    {
        //TODO: Validate jwt syntax
        
        var accessData = await OAuth2Service.RequestAccess(code);
        
        //TODO: Add modular oauth2 consumer system
        var userId = 1;

        var user = UserRepository.Get().First(x => x.Id == userId);

        user.AccessToken = accessData.AccessToken;
        user.RefreshToken = accessData.RefreshToken;
        user.RefreshTimestamp = DateTime.UtcNow.AddSeconds(accessData.ExpiresIn);
        
        UserRepository.Update(user);

        var authConfig = ConfigService.Get().Authentication;
        var tokenPair = await TokenHelper.GeneratePair(authConfig.MlAccessSecret, authConfig.MlAccessSecret, data =>
        {
            data.Add("userId", user.Id.ToString());
        });
        
        Response.Cookies.Append("ml-access", tokenPair.AccessToken);
        Response.Cookies.Append("ml-refresh", tokenPair.RefreshToken);
        Response.Cookies.Append("ml-timestamp", DateTimeOffset.UtcNow.AddSeconds(3600).ToUnixTimeSeconds().ToString());
        
        Response.Redirect("/");
    }

    [HttpGet("check")]
    [RequirePermission("meta.authenticated")]
    public async Task<CheckResponse> Check()
    {
        var perm = HttpContext.User as PermClaimsPrinciple;
        var user = perm!.CurrentModel;

        return new CheckResponse()
        {
            Email = user.Email,
            Username = user.Username,
            Permissions = perm.Permissions
        };
    }
}