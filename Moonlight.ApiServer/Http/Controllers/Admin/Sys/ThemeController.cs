using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.PermFilter;
using MoonCore.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Sys;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys;

[ApiController]
[Route("api/admin/system/theme")]
public class ThemeController : Controller
{
    [HttpPatch]
    [RequirePermission("admin.system.theme.update")]
    public async Task Patch([FromBody] UpdateThemeRequest request)
    {
        var themePath = PathBuilder.File("storage", "theme.json");

        await System.IO.File.WriteAllTextAsync(
            themePath,
            JsonSerializer.Serialize(request.Variables)
        );
    }
}