using Microsoft.JSInterop;
using MoonCore.Attributes;
using MoonCore.Helpers;

namespace Moonlight.Core.Services;

[Scoped]
public class HotKeyService : IAsyncDisposable
{
    private readonly IJSRuntime JsRuntime;

    public SmartEventHandler<string> HotKeyPressed { get; set; } = new();

    public HotKeyService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task Initialize()
    {
        var reference = DotNetObjectReference.Create(this);
        await JsRuntime.InvokeVoidAsync("moonlight.hotkeys.registerListener", reference);
    }

    [JSInvokable]
    public async void OnHotkeyPressed(string hotKey)
    {
        await HotKeyPressed.Invoke(hotKey);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("moonlight.keyListener.unregisterListener");
        }
        catch (Exception) { /* ignored */}
    }
}