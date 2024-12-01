using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Responses.Assets;

namespace Moonlight.ApiServer.Http.Controllers.Assets;

[ApiController]
[Route("api/assets")]
public class AssetsController : Controller
{
    private readonly AssetService AssetService;

    public AssetsController(AssetService assetService)
    {
        AssetService = assetService;
    }

    [HttpGet]
    public async Task<FrontendAssetResponse> Get()
    {
        return new FrontendAssetResponse()
        {
            CssFiles = AssetService.GetCssAssets(),
            JavascriptFiles = AssetService.GetJavascriptAssets(),
        };
    }
}