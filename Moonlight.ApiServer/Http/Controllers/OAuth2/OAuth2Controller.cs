using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.OAuth2.AuthServer;
using MoonCore.Extended.OAuth2.Models;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer.Http.Controllers.OAuth2;

[ApiController]
[Route("oauth2")]
public class OAuth2Controller : Controller
{
    private readonly OAuth2Service OAuth2Service;
    private readonly AuthService AuthService;
    private readonly DatabaseRepository<User> UserRepository;

    public OAuth2Controller(OAuth2Service oAuth2Service,
        AuthService authService, DatabaseRepository<User> userRepository)
    {
        OAuth2Service = oAuth2Service;
        AuthService = authService;
        UserRepository = userRepository;
    }

    [HttpGet("authorize")]
    public async Task Authorize(
        [FromQuery(Name = "response_type")] string responseType,
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri
    )
    {
        if (responseType != "code")
            throw new HttpApiException("Invalid response type", 400);

        if (!await OAuth2Service.IsValidAuthorization(clientId, redirectUri))
            throw new HttpApiException("Invalid authorization request", 400);

        Response.StatusCode = 200;
        await Response.WriteAsync(
            "<h1>Login lol</h1><br />" +
            "<br />" +
            "<br />" +
            "<form method=\"post\">" +
            "<label for=\"email\">Email:</label>" +
            "<input type=\"email\" id=\"email\" name=\"email\"><br>" +
            "<br>" +
            "<label for=\"password\">Password:</label>" +
            "<input type=\"password\" id=\"password\" name=\"password\"><br>" +
            "<br>" +
            "<input type=\"submit\" value=\"Submit\">" +
            "</form>"
        );
    }

    [HttpPost("authorize")]
    public async Task AuthorizePost(
        [FromQuery(Name = "response_type")] string responseType,
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromForm(Name = "email")] string email,
        [FromForm(Name = "password")] string password
    )
    {
        if (responseType != "code")
            throw new HttpApiException("Invalid response type", 400);

        if (!await OAuth2Service.IsValidAuthorization(clientId, redirectUri))
            throw new HttpApiException("Invalid authorization request", 400);

        var user = await AuthService.Login(email, password);

        var code = await OAuth2Service.GenerateCode(data => { data.Add("userId", user.Id.ToString()); });

        var redirectUrl = redirectUri +
                          $"?code={code}";

        Response.Redirect(redirectUrl);
    }

    [HttpPost("access")]
    public async Task<AccessData> Access(
        [FromForm(Name = "client_id")] string clientId,
        [FromForm(Name = "client_secret")] string clientSecret,
        [FromForm(Name = "redirect_uri")] string redirectUri,
        [FromForm(Name = "grant_type")] string grantType,
        [FromForm(Name = "code")] string code
        )
    {
        if (grantType != "authorization_code")
            throw new HttpApiException("Invalid grant type", 400);

        User? user = null;

        var access = await OAuth2Service.ValidateAccess(clientId, clientSecret, redirectUri, code, data =>
        {
            if (!data.TryGetValue("userId", out var userIdStr))
                return false;

            if (!int.TryParse(userIdStr, out var userId))
                return false;

            user = UserRepository.Get().FirstOrDefault(x => x.Id == userId);

            return user != null;
        }, data =>
        {
            data.Add("userId", user!.Id.ToString());
        });

        if (access == null)
            throw new HttpApiException("Unable to validate access", 400);

        return access;
    }
}