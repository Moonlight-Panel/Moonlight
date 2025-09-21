using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys;

[Authorize]
[ApiController]
[Route("api/admin/system/advanced")]
public class AdvancedController : Controller
{
    private readonly FrontendService FrontendService;

    public AdvancedController(FrontendService frontendService)
    {
        FrontendService = frontendService;
    }

    [HttpGet("frontend")]
    [Authorize(Policy = "permissions:admin.system.advanced.frontend")]
    public async Task FrontendAsync()
    {
        var stream = await FrontendService.GenerateZipAsync();
        await Results.File(stream, fileDownloadName: "frontend.zip").ExecuteAsync(HttpContext);
    }
}