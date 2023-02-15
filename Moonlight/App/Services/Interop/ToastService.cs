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
}