using Microsoft.JSInterop;

namespace Moonlight.Client.Services;

public class WindowService
{
    private readonly IJSRuntime JsRuntime;

    public WindowService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task Open(string url, string title, int height, int width)
     => await JsRuntime.InvokeVoidAsync("moonlight.window.open", url, title, height, width);

    public async Task Close()
        => await JsRuntime.InvokeVoidAsync("moonlight.window.closeCurrent");
}