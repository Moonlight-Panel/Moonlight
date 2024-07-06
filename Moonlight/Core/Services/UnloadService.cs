using Microsoft.JSInterop;
using MoonCore.Attributes;
using MoonCore.Helpers;

namespace Moonlight.Core.Services;

/// <summary>
/// This class provides an event to execute code when a user leaves the page
/// </summary>

[Scoped]
public class UnloadService
{
    public SmartEventHandler OnUnloaded { get; set; }
    
    private readonly IJSRuntime JsRuntime;
    private readonly ILogger<UnloadService> Logger;

    public UnloadService(IJSRuntime jsRuntime, ILogger<SmartEventHandler> eventHandlerLogger, ILogger<UnloadService> logger)
    {
        JsRuntime = jsRuntime;
        Logger = logger;

        OnUnloaded = new(eventHandlerLogger);
    }

    public async Task Initialize()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("moonlight.utils.registerUnload", DotNetObjectReference.Create(this));
        }
        catch (Exception e)
        {
            Logger.LogError("An error occured while registering unload event handler: {e}", e);
        }
    }

    [JSInvokable]
    public async Task Unload() => await OnUnloaded.Invoke();
}