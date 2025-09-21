using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Policy = "permissions:admin.system.overview")]
    public async Task<SystemOverviewResponse> GetOverviewAsync()
    {
        return new()
        {
            Uptime = await ApplicationService.GetUptimeAsync(),
            CpuUsage = await ApplicationService.GetCpuUsageAsync(),
            MemoryUsage = await ApplicationService.GetMemoryUsageAsync(),
            OperatingSystem = await ApplicationService.GetOsNameAsync()
        };
    }

    [HttpPost("shutdown")]
    [Authorize(Policy = "permissions:admin.system.shutdown")]
    public async Task ShutdownAsync()
    {
        await ApplicationService.ShutdownAsync();
    }
}