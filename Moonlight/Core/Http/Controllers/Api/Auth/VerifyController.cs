﻿using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using Moonlight.Core.Models.Enums;
using Moonlight.Core.Services;
using Moonlight.Core.Services.Utils;

namespace Moonlight.Core.Http.Controllers.Api.Auth;

[ApiController]
[Route("api/auth/verify")]
public class VerifyController : Controller
{
    private readonly IdentityService IdentityService;
    private readonly JwtService JwtService;

    public VerifyController(IdentityService identityService, JwtService jwtService)
    {
        IdentityService = identityService;
        JwtService = jwtService;
    }

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] string token)
    {
        await IdentityService.Authenticate(Request);

        if (!IdentityService.IsSignedIn)
            return Redirect("/login");

        if (!await JwtService.Validate(token))
            return Redirect("/login");

        var data = await JwtService.Decode(token);

        if (!data.ContainsKey("mailToVerify"))
            return Redirect("/login");

        var mailToVerify = data["mailToVerify"];

        if (mailToVerify != IdentityService.CurrentUser.Email)
        {
            Logger.Warn($"User {IdentityService.CurrentUser.Email} tried to mail verify {mailToVerify} via verify api endpoint", "security");
            return Redirect("/login");
        }

        IdentityService.Flags[UserFlag.MailVerified] = true;
        await IdentityService.SaveFlags();
        
        return Redirect("/");
    }
}