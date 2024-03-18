using Microsoft.JSInterop;
using MoonCore.Attributes;
using MoonCore.Helpers;

namespace Moonlight.Core.Services;

/// <summary>
/// This class provides a event to execute code when a user leaves the page
/// </summary>

[Scoped]
public class UnloadService
{
    public SmartEventHandler OnUnloaded { get; set; } = new();
    
    private readonly IJSRuntime JsRuntime;

    public UnloadService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task Initialize()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("moonlight.utils.registerUnload", DotNetObjectReference.Create(this));
        }
        catch (Exception e)
        {
            Logger.Error("An error occured while registering unload event handler");
            Logger.Error(e);
        }
    }

    [JSInvokable]
    public async Task Unload() => await OnUnloaded.Invoke();
}