using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Console;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models.Diagnose;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Responses.Admin.Sys;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys;

[ApiController]
[Route("api/admin/system")]
public class SystemController : Controller
{
    private readonly ApplicationService ApplicationService;
    private readonly IEnumerable<IDiagnoseProvider> DiagnoseProviders;
    private readonly DiagnoseService DiagnoseService;

    public SystemController(ApplicationService applicationService, IEnumerable<IDiagnoseProvider> diagnoseProviders, DiagnoseService diagnoseService)
    {
        ApplicationService = applicationService;
        DiagnoseProviders = diagnoseProviders;
        DiagnoseService = diagnoseService;
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

    [HttpGet("diagnose")]
    [RequirePermission("admin.system.diagnose")]
    public async Task<IActionResult> Diagnose()
    {
        var stream = new MemoryStream();
        
        await DiagnoseService.GenerateDiagnose(stream);
        
        return File(stream, "application/zip", "diagnose.zip");
    }
}