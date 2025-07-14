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
    public async Task<FrontendConfiguration> GetConfiguration()
        => await FrontendService.GetConfiguration();

    [HttpGet]
    public async Task<IResult> Index()
    {
        var content = await FrontendService.GenerateIndexHtml();

        return Results.Text(content, "text/html", Encoding.UTF8);
    }
}