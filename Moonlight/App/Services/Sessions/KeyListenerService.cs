using Microsoft.JSInterop;

namespace Moonlight.App.Services.Sessions;

public class KeyListenerService
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<KeyListenerService> _objRef;

    public event EventHandler<string> KeyPressed;

    public KeyListenerService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task Initialize()
    {
        _objRef = DotNetObjectReference.Create(this);
        await _jsRuntime.InvokeVoidAsync("moonlight.keyListener.register", _objRef);
    }

    [JSInvokable]
    public void OnKeyPress(string key)
    {
        KeyPressed?.Invoke(this, key);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("moonlight.keyListener.unregister", _objRef);
            _objRef.Dispose();
        }
        catch (Exception) { /* ignored */}
    }
}