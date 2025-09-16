using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Requests.Admin.Sys;
using Moonlight.Shared.Http.Responses.Admin.Sys;


namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys;

[ApiController]
[Route("api/admin/system/diagnose")]
[Authorize(Policy = "permissions:admin.system.diagnose")]
public class DiagnoseController : Controller
{
    private readonly DiagnoseService DiagnoseService;

    public DiagnoseController(DiagnoseService diagnoseService)
    {
        DiagnoseService = diagnoseService;
    }

    [HttpPost]
    public async Task<ActionResult> Diagnose([FromBody] GenerateDiagnoseRequest request)
    {
        var stream = await DiagnoseService.GenerateDiagnoseAsync(request.Providers);

        return File(stream, "application/zip", "diagnose.zip");
    }

    [HttpGet("providers")]
    public async Task<ActionResult<DiagnoseProvideResponse[]>> GetProviders()
    {
        return await DiagnoseService.GetProvidersAsync();
    }
}