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

    public AuthController(
        AppConfiguration configuration,
        ILogger<AuthController> logger,
        DatabaseRepository<User> userRepository
    )
    {
        Configuration = configuration;
        Logger = logger;
        UserRepository = userRepository;
    }

    [AllowAnonymous]
    [HttpGet("start")]
    public Task<LoginStartResponse> Start()
    {
        var response = new LoginStartResponse()
        {
            ClientId = Configuration.Authentication.OAuth2.ClientId,
            RedirectUri = Configuration.Authentication.OAuth2.AuthorizationRedirect ?? Configuration.PublicUrl,
            Endpoint = Configuration.Authentication.OAuth2.AuthorizationEndpoint ?? Configuration.PublicUrl + "/oauth2/authorize"
        };

        return Task.FromResult(response);
    }

    [AllowAnonymous]
    [HttpPost("complete")]
    public async Task<LoginCompleteResponse> Complete([FromBody] LoginCompleteRequest request)
    {
        // TODO: Make modular
        
        // Create http client to call the auth provider
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(Configuration.PublicUrl);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Configuration.Authentication.OAuth2.ClientSecret}");
        
        var httpApiClient = new HttpApiClient(httpClient);
        
        // Call the auth provider
        OAuth2HandleResponse handleData;

        try
        {
            handleData = await httpApiClient.PostJson<OAuth2HandleResponse>("oauth2/handle", new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", request.Code),
                    new KeyValuePair<string, string>("redirect_uri", Configuration.Authentication.OAuth2.AuthorizationRedirect ?? Configuration.PublicUrl),
                    new KeyValuePair<string, string>("client_id", Configuration.Authentication.OAuth2.ClientId)
                ]
            ));
        }
        catch (HttpApiException e)
        {
            if (e.Status == 400)
                Logger.LogTrace("The auth server returned an error: {e}", e);
            else
                Logger.LogCritical("The auth server returned an error: {e}", e);

            throw new HttpApiException("Unable to request user data", 500);
        }
        
        // Handle the returned data
        var userId = handleData.UserId;

        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            throw new HttpApiException("Unable to load user data", 500);
        
        //
        var permissions = JsonSerializer.Deserialize<string[]>(user.PermissionsJson) ?? [];
        
        // Generate token
        var securityTokenDescriptor = new SecurityTokenDescriptor()
        {
            Expires = DateTime.Now.AddDays(10), // TODO: config
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