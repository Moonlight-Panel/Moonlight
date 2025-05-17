using Microsoft.AspNetCore.Mvc;
using MoonCore.Attributes;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Requests.Admin.Sys;
using Moonlight.Shared.Http.Responses.Admin.Sys;
using Moonlight.Shared.Misc;


namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys;

[ApiController]
[Route("api/admin/system/diagnose")]
[RequirePermission("admin.system.diagnose")]
public class DiagnoseController : Controller
{
    private readonly DiagnoseService DiagnoseService;

    public DiagnoseController(DiagnoseService diagnoseService)
    {
        DiagnoseService = diagnoseService;
    }

    [HttpPost]
    public async Task Diagnose([FromBody] GenerateDiagnoseRequest request)
    {
        var stream = await DiagnoseService.GenerateDiagnose(request.Providers);
        
        await Results.Stream(
                stream,
                contentType: "application/zip",
                fileDownloadName: "diagnose.zip"
            )
            .ExecuteAsync(HttpContext);
    }

    [HttpGet("providers")]
    public async Task<DiagnoseProvideResponse[]> GetProviders()
    {
        return await DiagnoseService.GetProviders();
    }
}