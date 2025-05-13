using Microsoft.AspNetCore.Mvc;
using MoonCore.Attributes;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Requests.Admin.Sys;
using Moonlight.Shared.Http.Responses.Admin.Sys;
using Moonlight.Shared.Misc;


namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys;

[ApiController]
[Route("api/admin/system/diagnose")]
public class DiagnoseController : Controller
{
    private readonly DiagnoseService DiagnoseService;

    public DiagnoseController(DiagnoseService diagnoseService)
    {
        DiagnoseService = diagnoseService;
    }

    [HttpPost]
    [RequirePermission("admin.system.diagnose")]
    public async Task<IActionResult> Diagnose([FromBody] string[]? requestedDiagnoseProviders = null)
    {
        var stream = new MemoryStream();
        
        await DiagnoseService.GenerateDiagnose(stream, requestedDiagnoseProviders);
        
        return File(stream, "application/zip", "diagnose.zip");
    }

    [HttpGet("available")]
    [RequirePermission("admin.system.diagnose")]
    public async Task<SystemAvailableDiagnoseProviderResponse> GetAvailable()
    {
        var availableProviders = await DiagnoseService.GetAvailable();
        
        return new SystemAvailableDiagnoseProviderResponse()
        {
            AvailableProviders = availableProviders
        };
    }
}