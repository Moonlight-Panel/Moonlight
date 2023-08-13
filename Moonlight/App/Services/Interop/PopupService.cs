using Microsoft.JSInterop;

namespace Moonlight.App.Services.Interop;

public class PopupService
{
    private readonly IJSRuntime JsRuntime;

    public PopupService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task ShowCentered(string url, string title, int width = 500, int height = 500)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.popup.showCentered", url, title, width, height);
    }
}