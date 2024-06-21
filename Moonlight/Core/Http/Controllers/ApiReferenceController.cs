using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Services;
using Moonlight.Core.Attributes;
using Moonlight.Core.Configuration;
using Moonlight.Core.Models;
using Newtonsoft.Json;

namespace Moonlight.Core.Http.Controllers;

[ApiController]
[ApiDocument("internal")]
[Route("/api/core/reference")]
public class ApiReferenceController : Controller
{
    private readonly ConfigService<CoreConfiguration> ConfigService;

    public ApiReferenceController(ConfigService<CoreConfiguration> configService)
    {
        ConfigService = configService;
    }

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery][RegularExpression("^[a-z0-9_\\-]+$")] string document)
    {
        if (!ConfigService.Get().Development.EnableApiReference)
            return BadRequest("Api reference is disabled");
        
        var options = new ScalarOptions();
        var optionsJson = JsonConvert.SerializeObject(options, Formatting.Indented);

        var html = "<!doctype html>\n" +
                   "<html>\n" +
                   "<head>\n" +
                   "<title>Moonlight Api Reference</title>\n" +
                   "<meta charset=\"utf-8\" />\n" +
                   "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />\n" +
                   "<link rel=\"stylesheet\" type=\"text/css\" href=\"/api/core/asset/Core/css/scalar.css\" />\n"+
                   "</head>\n" +
                   "<body>\n" +
                   $"<script id=\"api-reference\" data-url=\"/api/core/reference/openapi/{document}\"></script>\n" +
                   "<script>\n" +
                   "var configuration =\n" +
                   $"{optionsJson}\n" +
                   "\n" +
                   "document.getElementById('api-reference').dataset.configuration =\n" +
                   "JSON.stringify(configuration)\n" +
                   "</script>\n" +
                   "<script src=\"https://cdn.jsdelivr.net/npm/@scalar/api-reference\"></script>\n" +
                   "</body>\n" +
                   "</html>";

        return Content(html, "text/html");
    }
}