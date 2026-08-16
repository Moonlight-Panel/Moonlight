using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moonlight.Api.Infrastructure.Helpers;
using Moonlight.ApiSdk.Features.Users;
using Moonlight.Shared.Features.Auth;

namespace Moonlight.Api.Features.Auth;

[Authorize]
[ApiController]
[Route("api/auth/info")]
public class InfoController : Controller
{
    private readonly IUserQueryService _userQueryService;

    public InfoController(IUserQueryService userQueryService)
    {
        _userQueryService = userQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<InfoDto>> GetAsync()
    {
        var userResult = await _userQueryService.FindByUsernameAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        if (userResult.IsFailed)
            return Problem(userResult.GetReasons(), statusCode: 500);

        return new InfoDto();
    }
}