using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.App.Attributes;
using Moonlight.ApiServer.App.Helpers;
using Moonlight.Shared.Http.Resources.Admin.Sys;

namespace Moonlight.ApiServer.App.Http.Controllers.Admin.Sys;

[Route("admin/system")]
[ApiController]
public class SystemController : Controller
{
    private readonly HostHelper HostHelper;

    public SystemController(HostHelper hostHelper)
    {
        HostHelper = hostHelper;
    }
    
    [HttpGet("info")]
    [RequirePermission("admin.system.info")]
    public async Task<ActionResult<SystemInfoResponse>> Info()
    {
        var response = new SystemInfoResponse()
        {
            CpuUsage = await HostHelper.GetCpuUsage(),
            MemoryUsage = await HostHelper.GetMemoryUsage(),
            OsName = await HostHelper.GetOsName(),
            Uptime = await HostHelper.GetUptime()
        };

        return Ok(response);
    }
}