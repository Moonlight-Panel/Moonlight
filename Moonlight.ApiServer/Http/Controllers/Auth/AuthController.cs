using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.Implementations.LocalAuth;
using Moonlight.Shared.Http.Responses.Auth;

namespace Moonlight.ApiServer.Http.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : Controller
{
    private readonly IAuthenticationSchemeProvider SchemeProvider;

    // Add schemes which should be offered to the client here
    private readonly string[] SchemeWhitelist = [LocalAuthConstants.AuthenticationScheme];

    public AuthController(IAuthenticationSchemeProvider schemeProvider)
    {
        SchemeProvider = schemeProvider;
    }

    [HttpGet]
    public async Task<AuthSchemeResponse[]> GetSchemes()
    {
        var schemes = await SchemeProvider.GetAllSchemesAsync();

        return schemes
            .Where(x => SchemeWhitelist.Contains(x.Name))
            .Select(scheme => new AuthSchemeResponse()
            {
                DisplayName = scheme.DisplayName ?? scheme.Name,
                Identifier = scheme.Name
            })
            .ToArray();
    }

    [HttpGet("{identifier:alpha}")]
    public async Task StartScheme([FromRoute] string identifier)
    {
        var scheme = await SchemeProvider.GetSchemeAsync(identifier);

        // The check for the whitelist ensures a user isn't starting an auth flow
        // which isn't meant for users
        if (scheme == null || !SchemeWhitelist.Contains(scheme.Name))
        {
            await Results
                .Problem(
                    "Invalid scheme identifier provided",
                    statusCode: 404
                )
                .ExecuteAsync(HttpContext);

            return;
        }

        await HttpContext.ChallengeAsync(
            scheme.Name,
            new AuthenticationProperties()
            {
                RedirectUri = "/"
            }
        );
    }

    [Authorize]
    [HttpGet("check")]
    public Task<AuthClaimResponse[]> Check()
    {
        var username = User.FindFirstValue(ClaimTypes.Name)!;
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        var userId = User.FindFirstValue("UserId")!;
        var permissions = User.FindFirstValue("Permissions")!;

        var claims = new List<AuthClaimResponse>()
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, id),
            new(ClaimTypes.Email, email),
            new("UserId", userId),
            new("Permissions", permissions)
        };

        return Task.FromResult(
            claims.ToArray()
        );
    }

    [HttpGet("logout")]
    public async Task Logout()
    {
        await HttpContext.SignOutAsync();
        await Results.Redirect("/").ExecuteAsync(HttpContext);
    }
}