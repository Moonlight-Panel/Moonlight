using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Services;
using Moonlight.ApiServer.App.Configuration;
using Moonlight.ApiServer.App.Models;

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
                   "<link rel=\"stylesheet\" type=\"text/css\" href=\"https://github.com/Moonlight-Panel/Moonlight/blob/2bb3b0fd48d4bd9ed8888f4b542819bd76fa0504/Moonlight/Assets/Core/css/scalar.css\" />\n"+
                   "</head>\n" +
                   "<body>\n" +
                   $"<script id=\"api-reference\" data-url=\"/apidocs/swagger/main\"></script>\n" +
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