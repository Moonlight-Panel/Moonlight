using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.App.Attributes;
using Moonlight.ApiServer.App.Services;

namespace Moonlight.ApiServer.App.Http.Controllers.Admin.Sys;

[ApiController]
[Route("/admin/system/diagnose")]
public class DiagnoseController : Controller
{
    private readonly DiagnoseService DiagnoseService;

    public DiagnoseController(DiagnoseService diagnoseService)
    {
        DiagnoseService = diagnoseService;
    }

    [HttpGet]
    [RequirePermission("admin.system.diagnose")]
    public async Task<ActionResult> Diagnose()
    {
        var stream = await DiagnoseService.GenerateReport();
        return File(stream, "application/zip");
    }
}