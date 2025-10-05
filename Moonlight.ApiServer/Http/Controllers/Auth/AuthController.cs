using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Interfaces;
using Moonlight.Shared.Http.Responses.Auth;

namespace Moonlight.ApiServer.Http.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : Controller
{
    private readonly IAuthenticationSchemeProvider SchemeProvider;
    private readonly IEnumerable<IAuthCheckExtension> Extensions;
    private readonly AppConfiguration Configuration;

    public AuthController(
        IAuthenticationSchemeProvider schemeProvider,
        IEnumerable<IAuthCheckExtension> extensions,
        AppConfiguration configuration
    )
    {
        SchemeProvider = schemeProvider;
        Extensions = extensions;
        Configuration = configuration;
    }

    [HttpGet]
    public async Task<AuthSchemeResponse[]> GetSchemesAsync()
    {
        var schemes = await SchemeProvider.GetAllSchemesAsync();

        var allowedSchemes = Configuration.Authentication.EnabledSchemes;

        return schemes
            .Where(x => allowedSchemes.Contains(x.Name))
            .Select(scheme => new AuthSchemeResponse()
            {
                DisplayName = scheme.DisplayName ?? scheme.Name,
                Identifier = scheme.Name
            })
            .ToArray();
    }

    [HttpGet("{identifier:alpha}")]
    public async Task StartSchemeAsync([FromRoute] string identifier)
    {
        // Validate identifier against our enable list
        var allowedSchemes = Configuration.Authentication.EnabledSchemes;

        if (!allowedSchemes.Contains(identifier))
        {
            await Results
                .Problem(
                    "Invalid scheme identifier provided",
                    statusCode: 404
                )
                .ExecuteAsync(HttpContext);

            return;
        }

        // Now we can check if it even exists
        var scheme = await SchemeProvider.GetSchemeAsync(identifier);

        if (scheme == null)
        {
            await Results
                .Problem(
                    "Invalid scheme identifier provided",
                    statusCode: 404
                )
                .ExecuteAsync(HttpContext);

            return;
        }

        // Everything fine, challenge the frontend
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
    public async Task<AuthClaimResponse[]> CheckAsync()
    {
        var username = User.FindFirstValue(ClaimTypes.Name)!;
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        var userId = User.FindFirstValue("UserId")!;
        var permissions = User.FindFirstValue("Permissions")!;

        // Create basic set of claims used by the frontend
        var claims = new List<AuthClaimResponse>()
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, id),
            new(ClaimTypes.Email, email),
            new("UserId", userId),
            new("Permissions", permissions)
        };

        // Enrich the frontend claims by extensions (used by plugins)
        foreach (var extension in Extensions)
        {
            claims.AddRange(
                await extension.GetFrontendClaimsAsync(User)
            );
        }

        return claims.ToArray();
    }

    [HttpGet("logout")]
    public async Task LogoutAsync()
    {
        await HttpContext.SignOutAsync();
        await Results.Redirect("/").ExecuteAsync(HttpContext);
    }
}