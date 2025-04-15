using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Interfaces;
using Moonlight.Shared.Http.Requests.Auth;
using Moonlight.Shared.Http.Responses.Auth;
using Moonlight.Shared.Http.Responses.OAuth2;

namespace Moonlight.ApiServer.Http.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : Controller
{
    private readonly AppConfiguration Configuration;
    private readonly ILogger<AuthController> Logger;
    private readonly DatabaseRepository<User> UserRepository;
    private readonly IOAuth2Provider OAuth2Provider;

    private readonly string RedirectUri;
    private readonly string EndpointUri;

    public AuthController(
        AppConfiguration configuration,
        ILogger<AuthController> logger,
        DatabaseRepository<User> userRepository,
        IOAuth2Provider oAuth2Provider
    )
    {
        UserRepository = userRepository;
        OAuth2Provider = oAuth2Provider;
        Configuration = configuration;
        Logger = logger;

        RedirectUri = string.IsNullOrEmpty(Configuration.Authentication.OAuth2.AuthorizationRedirect)
            ? Configuration.PublicUrl
            : Configuration.Authentication.OAuth2.AuthorizationRedirect;

        EndpointUri = string.IsNullOrEmpty(Configuration.Authentication.OAuth2.AuthorizationEndpoint)
            ? Configuration.PublicUrl + "/oauth2/authorize"
            : Configuration.Authentication.OAuth2.AuthorizationEndpoint;
    }

    [AllowAnonymous]
    [HttpGet("start")]
    public Task<LoginStartResponse> Start()
    {
        var response = new LoginStartResponse()
        {
            ClientId = Configuration.Authentication.OAuth2.ClientId,
            RedirectUri = RedirectUri,
            Endpoint = EndpointUri
        };

        return Task.FromResult(response);
    }

    [AllowAnonymous]
    [HttpPost("complete")]
    public async Task<LoginCompleteResponse> Complete([FromBody] LoginCompleteRequest request)
    {
        var user = await OAuth2Provider.Sync(request.Code);

        if (user == null)
            throw new HttpApiException("Unable to load user data", 500);

        //
        var permissions = JsonSerializer.Deserialize<string[]>(user.PermissionsJson) ?? [];

        // Generate token
        var securityTokenDescriptor = new SecurityTokenDescriptor()
        {
            Expires = DateTime.Now.AddYears(Configuration.Authentication.TokenDuration),
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
                    string.Join(";", permissions)
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
        var userIdClaim = User.Claims.First(x => x.Type == "userId");
        var userId = int.Parse(userIdClaim.Value);
        var user = await UserRepository.Get().FirstAsync(x => x.Id == userId);

        var permissions = JsonSerializer.Deserialize<string[]>(user.PermissionsJson) ?? [];

        return new()
        {
            Email = user.Email,
            Username = user.Username,
            Permissions = string.Join(";", permissions)
        };
    }
}