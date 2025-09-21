namespace Moonlight.Client.Services;

public class WindowService
{
    private readonly IJSRuntime JsRuntime;

    public WindowService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task OpenAsync(string url, string title, int height, int width)
     => await JsRuntime.InvokeVoidAsync("moonlight.window.open", url, title, height, width);

    public async Task CloseAsync()
        => await JsRuntime.InvokeVoidAsync("moonlight.window.closeCurrent");
}