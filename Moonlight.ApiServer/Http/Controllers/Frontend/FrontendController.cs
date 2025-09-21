using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Misc;

namespace Moonlight.ApiServer.Http.Controllers.Frontend;

[ApiController]
[Route("/")]
public class FrontendController : Controller
{
    private readonly FrontendService FrontendService;

    public FrontendController(FrontendService frontendService)
    {
        FrontendService = frontendService;
    }

    [HttpGet("frontend.json")]
    public async Task<FrontendConfiguration> GetConfigurationAsync()
        => await FrontendService.GetConfigurationAsync();

    [HttpGet]
    public async Task<IResult> IndexAsync()
    {
        var content = await FrontendService.GenerateIndexHtmlAsync();

        return Results.Text(content, "text/html", Encoding.UTF8);
    }
}