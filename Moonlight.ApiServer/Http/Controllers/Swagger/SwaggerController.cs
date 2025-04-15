using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Http.Controllers.Swagger;

[AllowAnonymous]
[Route("api/swagger")]
public class SwaggerController : Controller
{
    private readonly AppConfiguration Configuration;
    private readonly IServiceProvider ServiceProvider;

    public SwaggerController(
        AppConfiguration configuration,
        IServiceProvider serviceProvider
    )
    {
        Configuration = configuration;
        ServiceProvider = serviceProvider;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult> Get()
    {
        if (!Configuration.Development.EnableApiDocs)
            return BadRequest("Api docs are disabled");

        var options = new ApiDocsOptions();
        var optionsJson = JsonSerializer.Serialize(options);

        var html = await ComponentHelper.RenderComponent<SwaggerPage>(
            ServiceProvider,
            parameters =>
            {
                parameters.Add("Options", optionsJson);
            }
        );

        return Content(html, "text/html");
    }
}