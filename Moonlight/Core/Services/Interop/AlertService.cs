using Microsoft.JSInterop;

namespace Moonlight.Core.Services.Interop;

public class AlertService
{
    private readonly IJSRuntime JsRuntime;

    public AlertService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task Info(string title, string message)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.alerts.info", title, message);
    }
    
    public async Task Success(string title, string message)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.alerts.success", title, message);
    }
    
    public async Task Warning(string title, string message)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.alerts.warning", title, message);
    }
    
    public async Task Error(string title, string message)
    {
        await JsRuntime.InvokeVoidAsync("moonlight.alerts.error", title, message);
    }
    
    public async Task<string> Text(string title, string message)
    {
        return await JsRuntime.InvokeAsync<string>("moonlight.alerts.text", title, message);
    }
    
    public async Task<bool> YesNo(string title, string yes, string no)
    {
        try
        {
            return await JsRuntime.InvokeAsync<bool>("moonlight.alerts.yesno", title, yes, no);
        }
        catch (Exception)
        {
            return false;
        }
    }
}