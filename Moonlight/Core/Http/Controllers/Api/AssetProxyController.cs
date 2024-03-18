using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using Moonlight.Features.Theming.Services;

namespace Moonlight.Core.Http.Controllers.Api;

[ApiController]
[Route("api/assetproxy")]
public class AssetProxyController : Controller
{
    private readonly ThemeService ThemeService;

    public AssetProxyController(ThemeService themeService)
    {
        ThemeService = themeService;
    }

    [HttpGet("theme/{id}/js")]
    public async Task<ActionResult> GetThemeJs(int id)
    {
        var enabledThemes = await ThemeService.GetEnabled();
        var selectedTheme = enabledThemes.FirstOrDefault(x => x.Id == id);

        if (selectedTheme == null)
            return NotFound();

        try
        {
            using var httpClient = new HttpClient();
            var content = await httpClient.GetByteArrayAsync(selectedTheme.JsUrl);

            return File(content, "text/javascript");
        }
        catch (Exception e)
        {
            Logger.Warn($"Error proxying js for theme {id}");
            Logger.Warn(e);
            return Problem();
        }
    }
    
    [HttpGet("theme/{id}/css")]
    public async Task<ActionResult> GetThemeCss(int id)
    {
        var enabledThemes = await ThemeService.GetEnabled();
        var selectedTheme = enabledThemes.FirstOrDefault(x => x.Id == id);

        if (selectedTheme == null)
            return NotFound();

        try
        {
            using var httpClient = new HttpClient();
            var content = await httpClient.GetByteArrayAsync(selectedTheme.CssUrl);

            return File(content, "text/css");
        }
        catch (Exception e)
        {
            Logger.Warn($"Error proxying css for theme {id}");
            Logger.Warn(e);
            return Problem();
        }
    }
}