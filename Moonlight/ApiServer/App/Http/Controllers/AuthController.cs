using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.App.Attributes;
using Moonlight.ApiServer.App.Services;
using Moonlight.Shared.Http.Requests.Auth;
using Moonlight.Shared.Http.Resources.Auth;

namespace Moonlight.ApiServer.App.Http.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : Controller
{
    private readonly UserService UserService;

    public AuthController(UserService userService)
    {
        UserService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        var token = await UserService.Register(request.Username, request.Email, request.Password);

        return Ok(new RegisterResponse()
        {
            Token = token
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var token = await UserService.Login(request.Identifier, request.Password, request.TwoFactorCode);

        return Ok(new RegisterResponse()
        {
            Token = token
        });
    }

    [HttpGet("check")]
    [RequirePermission("meta.authenticated")]
    public Task<ActionResult> Check() => Task.FromResult<ActionResult>(Ok());
}