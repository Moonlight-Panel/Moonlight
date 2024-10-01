using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.Attributes;
using Moonlight.ApiServer.Helpers.Authentication;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Requests.Auth;
using Moonlight.Shared.Http.Responses.Auth;

namespace Moonlight.ApiServer.Http.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : Controller
{
    private readonly AuthService AuthService;

    public AuthController(AuthService authService)
    {
        AuthService = authService;
    }

    [HttpPost("login")]
    public async Task<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var user = await AuthService.Login(request.Email, request.Password);

        return new LoginResponse()
        {
            Token = await AuthService.GenerateToken(user)
        };
    }

    [HttpPost("register")]
    public async Task<RegisterResponse> Register([FromBody] RegisterRequest request)
    {
        var user = await AuthService.Register(
            request.Username,
            request.Email,
            request.Password
        );
        
        return new RegisterResponse()
        {
            Token = await AuthService.GenerateToken(user)
        };
    }
    
    [HttpGet("check")]
    [RequirePermission("meta.authenticated")]
    public async Task<CheckResponse> Check()
    {
        var perm = HttpContext.User as PermClaimsPrinciple;
        var user = perm!.CurrentModel;

        return new CheckResponse()
        {
            Email = user.Email,
            Username = user.Username,
            Permissions = perm.Permissions
        };
    }
}