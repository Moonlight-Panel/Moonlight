using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.OAuth2.AuthServer;
using MoonCore.Extended.OAuth2.Models;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Responses.OAuth2;

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

        var code = await OAuth2Service.GenerateCode(data => { data.Add("userId", user.Id); });

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
            if (!data.TryGetValue("userId", out var userIdStr) || !userIdStr.TryGetInt32(out var userId))
                return false;

            user = UserRepository.Get().FirstOrDefault(x => x.Id == userId);

            return user != null;
        }, data => { data.Add("userId", user!.Id); });

        if (access == null)
            throw new HttpApiException("Unable to validate access", 400);

        return access;
    }

    [HttpPost("refresh")]
    public async Task<RefreshData> Refresh(
        [FromForm(Name = "grant_type")] string grantType,
        [FromForm(Name = "refresh_token")] string refreshToken
    )
    {
        if (grantType != "refresh_token")
            throw new HttpApiException("Invalid grant type", 400);

        var refreshData = await OAuth2Service.RefreshAccess(refreshToken, (refreshTokenData, newTokenData) =>
        {
            // Check if the userId is present in the refresh token
            if (!refreshTokenData.TryGetValue("userId", out var userIdStr) || !userIdStr.TryGetInt32(out var userId))
                return false;
            
            // Load user from database if existent
            var user = UserRepository
                .Get()
                .FirstOrDefault(x => x.Id == userId);
            
            if (user == null)
                return false;
            
            newTokenData.Add("userId", user.Id);
            return true;
        });
        
        if(refreshData == null)
            throw new HttpApiException("Unable to validate refresh", 400);

        return refreshData;
    }

    [HttpGet("info")]
    public async Task<InfoResponse> Info()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            throw new HttpApiException("Authorization header is missing", 400);

        var authHeader = Request.Headers["Authorization"].First() ?? "";

        if (string.IsNullOrEmpty(authHeader))
            throw new HttpApiException("Authorization header is missing", 400);

        User? currentUser = null;

        var isValid = await OAuth2Service.IsValidAccessToken(
            authHeader,
            data =>
            {
                // Check if the userId is present in the access token
                if (!data.TryGetValue("userId", out var userIdStr) || !userIdStr.TryGetInt32(out var userId))
                    return false;

                currentUser = UserRepository
                    .Get()
                    .FirstOrDefault(x => x.Id == userId);

                if (currentUser == null)
                    return false;

                return true;
            }
        );

        if (!isValid)
            throw new HttpApiException("Invalid access token", 401);

        if (currentUser == null)
            throw new HttpApiException("Invalid access token", 401);

        return new InfoResponse()
        {
            Username = currentUser.Username,
            Email = currentUser.Email
        };
    }
}