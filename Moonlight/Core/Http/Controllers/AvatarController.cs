﻿using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Abstractions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Attributes;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Services;

namespace Moonlight.Core.Http.Controllers;

[ApiController]
[ApiDocument("internal")]
[Route("api/core/avatar")]
public class AvatarController : Controller
{
    private readonly Repository<User> UserRepository;
    private readonly ConfigService<CoreConfiguration> ConfigService;
    private readonly IdentityService IdentityService;

    public AvatarController(
        Repository<User> userRepository,
        IdentityService identityService,
        ConfigService<CoreConfiguration> configService)
    {
        UserRepository = userRepository;
        IdentityService = identityService;
        ConfigService = configService;
    }
    
    [HttpGet]
    public async Task<ActionResult> Get()
    {
        if (!Request.Cookies.ContainsKey("token"))
            return StatusCode(403);

        var token = Request.Cookies["token"];
        await IdentityService.Authenticate(token!);

        if (!IdentityService.IsLoggedIn)
            return StatusCode(403);
        
        return File(await GetAvatar(IdentityService.CurrentUser), "image/jpeg");
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Get(int id)
    {
        if (!Request.Cookies.ContainsKey("token"))
            return StatusCode(403);

        var token = Request.Cookies["token"];
        await IdentityService.Authenticate(token!);

        if (!IdentityService.IsLoggedIn)
            return StatusCode(403);

        if (ConfigService.Get().Security.EnforceAvatarPrivacy && // Do we need to enforce privacy?
            id != IdentityService.CurrentUser.Id && // is the user not viewing his own image?
            IdentityService.CurrentUser.Permissions < 1000) // and not an admin?
        {
            return StatusCode(403);
        }

        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Id == id);

        if (user == null)
            return NotFound();

        return File(await GetAvatar(user), "image/jpeg");
    }

    private async Task<Stream> GetAvatar(User user)
    {
        try
        {
            var hash = Hash(user.Email.ToLower());

            using var httpClient = new HttpClient();
            var stream = await httpClient.GetStreamAsync($"https://gravatar.com/avatar/{hash}");

            return stream;
        }
        catch (Exception e)
        {
            if(e is HttpRequestException requestException && requestException.InnerException is IOException ioException)
                Logger.Warn($"Unable to fetch gravatar for user {user.Id}. Is moonlight inside a proxy requiring network?: {ioException.Message}");
            else
            {
                Logger.Warn($"Unable to fetch gravatar for user {user.Id}");
                Logger.Warn(e);
            }

            return new MemoryStream();
        }
    }
    
    private string Hash(string input)
    {
        var md5 = MD5.Create();
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hash = md5.ComputeHash(inputBytes);

        var sb = new StringBuilder();
        foreach (var t in hash) sb.Append(t.ToString("X2"));
        return sb.ToString().ToLower();
    }
}