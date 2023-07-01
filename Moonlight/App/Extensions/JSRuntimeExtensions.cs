using Microsoft.JSInterop;

namespace Moonlight.App.Extensions;

public static class JSRuntimeExtensions
{
    public static async Task InvokeVoidSafeAsync(this IJSRuntime jsRuntime, string method, params object[] args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(method, args);
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    public static void InvokeVoidSafe(this IJSRuntime jsRuntime, string method, params object[] args)
    {
        try
        {
            jsRuntime.InvokeVoidAsync(method, args);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}