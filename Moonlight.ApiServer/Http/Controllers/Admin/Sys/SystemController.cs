using Microsoft.AspNetCore.Mvc;
using MoonCore.Attributes;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Responses.Admin.Sys;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys;

[ApiController]
[Route("api/admin/system")]
public class SystemController : Controller
{
    private readonly ApplicationService ApplicationService;

    public SystemController(ApplicationService applicationService)
    {
        ApplicationService = applicationService;
    }

    [HttpGet]
    [RequirePermission("admin.system.overview")]
    public async Task<SystemOverviewResponse> GetOverview()
    {
        return new()
        {
            Uptime = await ApplicationService.GetUptime(),
            CpuUsage = await ApplicationService.GetCpuUsage(),
            MemoryUsage = await ApplicationService.GetMemoryUsage(),
            OperatingSystem = await ApplicationService.GetOsName()
        };
    }

    [HttpPost("shutdown")]
    [RequirePermission("admin.system.shutdown")]
    public async Task Shutdown()
    {
        await ApplicationService.Shutdown();
    }
}