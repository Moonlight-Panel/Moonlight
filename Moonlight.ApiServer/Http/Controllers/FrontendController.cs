using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Misc;

namespace Moonlight.ApiServer.Http.Controllers;

[ApiController]
public class FrontendController : Controller
{
    private readonly FrontendService FrontendService;

    public FrontendController(FrontendService frontendService)
    {
        FrontendService = frontendService;
    }

    [HttpGet("frontend.json")]
    public async Task<FrontendConfiguration> GetConfiguration()
        => await FrontendService.GetConfiguration();
}