using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Services;
using Moonlight.ApiServer.App.Models;
using Moonlight.Shared.Models;

namespace Moonlight.ApiServer.App.Http.Controllers;

[Route("apidocs")]
[ApiController]
public class ApiDocsController : Controller
{
    private readonly ConfigService<AppConfiguration> ConfigService;

    public ApiDocsController(ConfigService<AppConfiguration> configService)
    {
        ConfigService = configService;
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        if (!ConfigService.Get().Development.EnableApiDocs)
            return BadRequest("Api docs are disabled");
        
        var options = new ApiDocsOptions();
        var optionsJson = JsonSerializer.Serialize(options);

        //TODO: Replace the css link with a better one
        
        var html = "<!doctype html>\n" +
                   "<html>\n" +
                   "<head>\n" +
                   "<title>Moonlight Api Reference</title>\n" +
                   "<meta charset=\"utf-8\" />\n" +
                   "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />\n" +
                   "</head>\n" +
                   "<body>\n" +
                   "<script id=\"api-reference\" data-url=\"/apidocs/swagger/main\"></script>\n" +
                   "<script>\n" +
                   "var configuration =\n" +
                   $"{optionsJson}\n" +
                   "\n" +
                   "document.getElementById('api-reference').dataset.configuration =\n" +
                   "JSON.stringify(configuration)\n" +
                   "</script>\n" +
                   "<script src=\"https://cdn.jsdelivr.net/npm/@scalar/api-reference\"></script>\n" +
                   "<style>.light-mode {\n    --scalar-background-1: #fff;\n    --scalar-background-2: #f8fafc;\n    --scalar-background-3: #e7e7e7;\n    --scalar-background-accent: #8ab4f81f;\n    --scalar-color-1: #000;\n    --scalar-color-2: #6b7280;\n    --scalar-color-3: #9ca3af;\n    --scalar-color-accent: #00c16a;\n    --scalar-border-color: #e5e7eb;\n    --scalar-color-green: #069061;\n    --scalar-color-red: #ef4444;\n    --scalar-color-yellow: #f59e0b;\n    --scalar-color-blue: #1d4ed8;\n    --scalar-color-orange: #fb892c;\n    --scalar-color-purple: #6d28d9;\n    --scalar-button-1: #000;\n    --scalar-button-1-hover: rgba(0, 0, 0, 0.9);\n    --scalar-button-1-color: #fff;\n}\n.dark-mode {\n    --scalar-background-1: #020420;\n    --scalar-background-2: #121a31;\n    --scalar-background-3: #1e293b;\n    --scalar-background-accent: #8ab4f81f;\n    --scalar-color-1: #fff;\n    --scalar-color-2: #cbd5e1;\n    --scalar-color-3: #94a3b8;\n    --scalar-color-accent: #00dc82;\n    --scalar-border-color: #1e293b;\n    --scalar-color-green: #069061;\n    --scalar-color-red: #f87171;\n    --scalar-color-yellow: #fde68a;\n    --scalar-color-blue: #60a5fa;\n    --scalar-color-orange: #fb892c;\n    --scalar-color-purple: #ddd6fe;\n    --scalar-button-1: hsla(0, 0%, 100%, 0.9);\n    --scalar-button-1-hover: hsla(0, 0%, 100%, 0.8);\n    --scalar-button-1-color: #000;\n}\n.dark-mode .t-doc__sidebar,\n.light-mode .t-doc__sidebar {\n    --scalar-sidebar-background-1: var(--scalar-background-1);\n    --scalar-sidebar-color-1: var(--scalar-color-1);\n    --scalar-sidebar-color-2: var(--scalar-color-3);\n    --scalar-sidebar-border-color: var(--scalar-border-color);\n    --scalar-sidebar-item-hover-background: transparent;\n    --scalar-sidebar-item-hover-color: var(--scalar-color-1);\n    --scalar-sidebar-item-active-background: transparent;\n    --scalar-sidebar-color-active: var(--scalar-color-accent);\n    --scalar-sidebar-search-background: transparent;\n    --scalar-sidebar-search-color: var(--scalar-color-3);\n    --scalar-sidebar-search-border-color: var(--scalar-border-color);\n    --scalar-sidebar-indent-border: var(--scalar-border-color);\n    --scalar-sidebar-indent-border-hover: var(--scalar-color-1);\n    --scalar-sidebar-indent-border-active: var(--scalar-color-accent);\n}\n.scalar-card .request-card-footer {\n    --scalar-background-3: var(--scalar-background-2);\n    --scalar-button-1: #0f172a;\n    --scalar-button-1-hover: rgba(30, 41, 59, 0.5);\n    --scalar-button-1-color: #fff;\n}\n.scalar-card .show-api-client-button {\n    border: 1px solid #334155 !important;\n}</style>\n" +
                   "</body>\n" +
                   "</html>";

        return Content(html, "text/html");
    }
    
    
}