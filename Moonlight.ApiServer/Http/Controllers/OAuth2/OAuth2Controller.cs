using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.Shared.Http.Responses.OAuth2;

namespace Moonlight.ApiServer.Http.Controllers.OAuth2;

[ApiController]
[Route("oauth2")]
public partial class OAuth2Controller : Controller
{
    private readonly AppConfiguration Configuration;
    private readonly DatabaseRepository<User> UserRepository;

    private readonly string ExpectedRedirectUri;

    public OAuth2Controller(AppConfiguration configuration, DatabaseRepository<User> userRepository)
    {
        Configuration = configuration;
        UserRepository = userRepository;

        ExpectedRedirectUri = string.IsNullOrEmpty(Configuration.Authentication.OAuth2.AuthorizationRedirect)
            ? Configuration.PublicUrl
            : Configuration.Authentication.OAuth2.AuthorizationRedirect;
    }

    [AllowAnonymous]
    [HttpGet("authorize")]
    public async Task Authorize(
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "response_type")] string responseType,
        [FromQuery(Name = "view")] string view = "login"
    )
    {
        if (!Configuration.Authentication.EnableLocalOAuth2)
            throw new HttpApiException("Local OAuth2 has been disabled", 403);

        if (Configuration.Authentication.OAuth2.ClientId != clientId ||
            redirectUri != ExpectedRedirectUri ||
            responseType != "code")
        {
            throw new HttpApiException("Invalid oauth2 request", 400);
        }

        Response.StatusCode = 200;

        if (view == "register")
        {
            var html = await ComponentHelper.RenderComponent<Register>(HttpContext.RequestServices, parameters =>
            {
                parameters.Add("ClientId", clientId);
                parameters.Add("RedirectUri", redirectUri);
                parameters.Add("ResponseType", responseType);
            });

            await Response.WriteAsync(html);
        }
        else
        {
            var html = await ComponentHelper.RenderComponent<Login>(HttpContext.RequestServices, parameters =>
            {
                parameters.Add("ClientId", clientId);
                parameters.Add("RedirectUri", redirectUri);
                parameters.Add("ResponseType", responseType);
            });

            await Response.WriteAsync(html);
        }
    }

    [AllowAnonymous]
    [HttpPost("authorize")]
    public async Task AuthorizePost(
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "response_type")] string responseType,
        [FromForm(Name = "email")] [EmailAddress(ErrorMessage = "You need to provide a valid email address")] string email,
        [FromForm(Name = "password")] string password,
        [FromForm(Name = "username")] string username = "",
        [FromQuery(Name = "view")] string view = "login"
    )
    {
        if (!Configuration.Authentication.EnableLocalOAuth2)
            throw new HttpApiException("Local OAuth2 has been disabled", 403);

        if (Configuration.Authentication.OAuth2.ClientId != clientId ||
            redirectUri != ExpectedRedirectUri ||
            responseType != "code")
        {
            throw new HttpApiException("Invalid oauth2 request", 400);
        }

        if (view == "register" && string.IsNullOrEmpty(username))
            throw new HttpApiException("You need to provide a username", 400);

        string? errorMessage = null;

        try
        {
            if (view == "register")
            {
                var user = await Register(username, email, password);
                var code = await GenerateCode(user);

                Response.Redirect($"{redirectUri}?code={code}");
                return;
            }
            else
            {
                var user = await Login(email, password);
                var code = await GenerateCode(user);

                Response.Redirect($"{redirectUri}?code={code}");
                return;
            }
        }
        catch (HttpApiException e)
        {
            errorMessage = e.Title;
        }

        Response.StatusCode = 200;

        if (view == "register")
        {
            var html = await ComponentHelper.RenderComponent<Register>(HttpContext.RequestServices, parameters =>
            {
                parameters.Add("ClientId", clientId);
                parameters.Add("RedirectUri", redirectUri);
                parameters.Add("ResponseType", responseType);
                parameters.Add("ErrorMessage", errorMessage!);
            });

            await Response.WriteAsync(html);
        }
        else
        {
            var html = await ComponentHelper.RenderComponent<Login>(HttpContext.RequestServices, parameters =>
            {
                parameters.Add("ClientId", clientId);
                parameters.Add("RedirectUri", redirectUri);
                parameters.Add("ResponseType", responseType);
                parameters.Add("ErrorMessage", errorMessage!);
            });

            await Response.WriteAsync(html);
        }
    }

    [AllowAnonymous]
    [HttpPost("handle")]
    public async Task<OAuth2HandleResponse> Handle(
        [FromForm(Name = "grant_type")] string grantType,
        [FromForm(Name = "code")] string code,
        [FromForm(Name = "redirect_uri")] string redirectUri,
        [FromForm(Name = "client_id")] string clientId
    )
    {
        if (!Configuration.Authentication.EnableLocalOAuth2)
            throw new HttpApiException("Local OAuth2 has been disabled", 403);

        // Check header
        if (!Request.Headers.ContainsKey("Authorization"))
            throw new HttpApiException("You are missing the Authorization header", 400);

        var authorizationHeaderValue = Request.Headers["Authorization"].FirstOrDefault() ?? "";

        if (authorizationHeaderValue != $"Basic {Configuration.Authentication.OAuth2.ClientSecret}")
            throw new HttpApiException("Invalid Authorization header value", 400);

        // Check form
        if (grantType != "authorization_code")
            throw new HttpApiException("Invalid grant type provided", 400);

        if (clientId != Configuration.Authentication.OAuth2.ClientId)
            throw new HttpApiException("Invalid client id provided", 400);

        if (redirectUri != ExpectedRedirectUri)
            throw new HttpApiException("Invalid redirect uri provided", 400);

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

        ClaimsPrincipal? codeData;

        try
        {
            codeData = jwtSecurityTokenHandler.ValidateToken(code, new TokenValidationParameters()
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    Configuration.Authentication.OAuth2.Secret
                )),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateAudience = false,
                ValidateIssuer = false
            }, out _);
        }
        catch (SecurityTokenException)
        {
            throw new HttpApiException("Invalid code provided", 400);
        }

        if (codeData == null)
            throw new HttpApiException("Invalid code provided", 400);

        var userIdClaim = codeData.Claims.FirstOrDefault(x => x.Type == "id");

        if (userIdClaim == null)
            throw new HttpApiException("Malformed code provided", 400);

        if (!int.TryParse(userIdClaim.Value, out var userId))
            throw new HttpApiException("Malformed code provided", 400);

        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Id == userId);

        if (user == null)
            throw new HttpApiException("Malformed code provided", 400);

        return new()
        {
            UserId = user.Id
        };
    }

    private Task<string> GenerateCode(User user)
    {
        var securityTokenDescriptor = new SecurityTokenDescriptor()
        {
            Expires = DateTime.Now.AddMinutes(1),
            IssuedAt = DateTime.Now,
            NotBefore = DateTime.Now.AddMinutes(-1),
            Claims = new Dictionary<string, object>()
            {
                {
                    "id",
                    user.Id
                }
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration.Authentication.OAuth2.Secret)
                ),
                SecurityAlgorithms.HmacSha256
            )
        };

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);

        return Task.FromResult(
            jwtSecurityTokenHandler.WriteToken(securityToken)
        );
    }

    private async Task<User> Register(string username, string email, string password)
    {
        if (await UserRepository.Get().AnyAsync(x => x.Username == username))
            throw new HttpApiException("A account with that username already exists", 400);

        if (await UserRepository.Get().AnyAsync(x => x.Email == email))
            throw new HttpApiException("A account with that email already exists", 400);

        if (!UsernameRegex().IsMatch(username))
            throw new HttpApiException("The username is only allowed to be contained out of small characters and numbers", 400);
        
        var user = new User()
        {
            Username = username,
            Email = email,
            Password = HashHelper.Hash(password),
        };
        
        if (Configuration.Authentication.OAuth2.FirstUserAdmin)
        {
            var userCount = await UserRepository.Get().CountAsync();
            
            if (userCount == 0)
                user.PermissionsJson = "[\"*\"]";
            
        }

        return await UserRepository.Add(user);
    }

    private async Task<User> Login(string email, string password)
    {
        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
            throw new HttpApiException("Invalid combination of email and password", 400);

        if (!HashHelper.Verify(password, user.Password))
            throw new HttpApiException("Invalid combination of email and password", 400);

        return user;
    }

    [GeneratedRegex("^[a-z][a-z0-9]*$")]
    private static partial Regex UsernameRegex();
}