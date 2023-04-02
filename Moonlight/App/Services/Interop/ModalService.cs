using Microsoft.JSInterop;

namespace Moonlight.App.Services.Interop;

public class ModalService
{
    private readonly IJSRuntime JsRuntime;

    public ModalService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task Show(string name)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.modals.show", name);
    }

    public async Task Hide(string name)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.modals.hide", name);
    }
}