using Microsoft.JSInterop;
using Moonlight.App.Helpers;

namespace Moonlight.App.Services.Interop;

public class AdBlockService
{
    private readonly IJSRuntime JsRuntime;

    public AdBlockService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task<bool> Detect()
    {
        try
        {
            return await JsRuntime.InvokeAsync<bool>("moonlight.utils.vendo"); // lat. vendo = advertisement xd
        }
        catch (Exception e)
        {
            Logger.Warn("An unexpected error occured while trying to detect possible ad blockers");
            Logger.Warn(e);

            return false;
        }
    }
}