using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Interfaces;
using Moonlight.Shared.Http.Requests.Auth;
using Moonlight.Shared.Http.Responses.Auth;

namespace Moonlight.ApiServer.Http.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : Controller
{
    private readonly AppConfiguration Configuration;
    private readonly DatabaseRepository<User> UserRepository;
    private readonly IOAuth2Provider OAuth2Provider;

    public AuthController(
        AppConfiguration configuration,
        DatabaseRepository<User> userRepository,
        IOAuth2Provider oAuth2Provider
    )
    {
        UserRepository = userRepository;
        OAuth2Provider = oAuth2Provider;
        Configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet("start")]
    public async Task<LoginStartResponse> Start()
    {
        var url = await OAuth2Provider.Start();

        return new LoginStartResponse()
        {
            Url = url
        };
    }

    [AllowAnonymous]
    [HttpPost("complete")]
    public async Task<LoginCompleteResponse> Complete([FromBody] LoginCompleteRequest request)
    {
        var user = await OAuth2Provider.Complete(request.Code);

        if (user == null)
            throw new HttpApiException("Unable to load user data", 500);

        // Generate token
        var securityTokenDescriptor = new SecurityTokenDescriptor()
        {
            Expires = DateTime.Now.AddHours(Configuration.Authentication.TokenDuration),
            IssuedAt = DateTime.Now,
            NotBefore = DateTime.Now.AddMinutes(-1),
            Claims = new Dictionary<string, object>()
            {
                {
                    "userId",
                    user.Id
                },
                {
                    "permissions",
                    string.Join(";", user.Permissions)
                }
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration.Authentication.Secret)
                ),
                SecurityAlgorithms.HmacSha256
            ),
            Issuer = Configuration.PublicUrl,
            Audience = Configuration.PublicUrl
        };

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);

        var jwt = jwtSecurityTokenHandler.WriteToken(securityToken);

        return new()
        {
            AccessToken = jwt
        };
    }

    [Authorize]
    [HttpGet("check")]
    public async Task<CheckResponse> Check()
    {
        var userIdStr = User.FindFirstValue("userId")!;
        var userId = int.Parse(userIdStr);
        var user = await UserRepository.Get().FirstAsync(x => x.Id == userId);

        return new()
        {
            Email = user.Email,
            Username = user.Username,
            Permissions = user.Permissions
        };
    }
}