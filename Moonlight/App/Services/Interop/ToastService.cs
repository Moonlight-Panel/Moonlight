using Microsoft.JSInterop;

namespace Moonlight.App.Services.Interop;

public class ToastService
{
    private readonly IJSRuntime JsRuntime;

    public ToastService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }
    
    public async Task Info(string message)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.toasts.info", message);
    }
    
    public async Task Error(string message)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.toasts.error", message);
    }
    
    public async Task Warning(string message)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.toasts.warning", message);
    }
    
    public async Task Success(string message)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.toasts.success", message);
    }

    public async Task CreateProcessToast(string id, string text)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.toasts.create", id, text);
    }

    public async Task UpdateProcessToast(string id, string text)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.toasts.modify", id, text);
    }
    
    public async Task RemoveProcessToast(string id)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.toasts.remove", id);
    }
}