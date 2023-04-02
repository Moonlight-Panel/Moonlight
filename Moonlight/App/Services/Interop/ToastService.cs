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
        await JsRuntime.InvokeVoidAsync("showInfoToast", message);
    }
    
    public async Task Error(string message)
    {
        await JsRuntime.InvokeVoidAsync("showErrorToast", message);
    }
    
    public async Task Warning(string message)
    {
        await JsRuntime.InvokeVoidAsync("showWarningToast", message);
    }
    
    public async Task Success(string message)
    {
        await JsRuntime.InvokeVoidAsync("showSuccessToast", message);
    }

    public async Task CreateProcessToast(string id, string text)
    {
        await JsRuntime.InvokeVoidAsync("createToast", id, text);
    }

    public async Task UpdateProcessToast(string id, string text)
    {
        await JsRuntime.InvokeVoidAsync("modifyToast", id, text);
    }
    
    public async Task RemoveProcessToast(string id)
    {
        await JsRuntime.InvokeVoidAsync("removeToast", id);
    }
}