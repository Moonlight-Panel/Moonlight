using Microsoft.AspNetCore.Mvc;
using MoonCore.Abstractions;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Models.Enums;

using Moonlight.Core.Services;
using Moonlight.Core.Services.Utils;

namespace Moonlight.Core.Http.Controllers.Api.Auth;

[ApiController]
[Route("api/auth/reset")]
public class ResetController : Controller
{
    private readonly Repository<User> UserRepository;
    private readonly IdentityService IdentityService;
    private readonly JwtService JwtService;

    public ResetController(Repository<User> userRepository, IdentityService identityService, JwtService jwtService)
    {
        UserRepository = userRepository;
        IdentityService = identityService;
        JwtService = jwtService;
    }

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] string token)
    {
        // Validate token
        
        if (!await JwtService.Validate(token))
            return Redirect("/password-reset");

        var data = await JwtService.Decode(token);

        if (!data.ContainsKey("accountToReset"))
            return Redirect("/password-reset");

        var userId = int.Parse(data["accountToReset"]);
        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Id == userId);

        // User may have been deleted, so we check here
        
        if (user == null)
            return Redirect("/password-reset");
        
        // In order to allow the user to get access to the change password screen
        // we need to authenticate him so we can read his flags.
        // That's why we are creating a session here

        var sessionToken = await IdentityService.GenerateToken(user);
        
        // Authenticate the current identity service instance in order to
        // get access to the flags field.
        await IdentityService.Authenticate(sessionToken);
        IdentityService.Flags[UserFlag.PasswordPending] = true;
        await IdentityService.SaveFlags();
        
        // Make the user login so he can reach the change password screen
        Response.Cookies.Append("token", sessionToken);
        return Redirect("/");
    }
}