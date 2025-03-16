using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.PermFilter;
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
    [RequirePermission("admin.system.advanced.frontend")]
    public async Task Frontend()
    {
        var stream = await FrontendService.GenerateZip();
        await Results.File(stream, fileDownloadName: "frontend.zip").ExecuteAsync(HttpContext);
    }
}