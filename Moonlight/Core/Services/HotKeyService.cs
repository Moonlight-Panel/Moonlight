using Microsoft.JSInterop;
using MoonCore.Attributes;
using MoonCore.Helpers;
using Moonlight.Core.Models.Abstractions;

namespace Moonlight.Core.Services;

[Scoped]
public class HotKeyService
{
    private readonly IJSRuntime JsRuntime;
    private readonly List<HotKeyModel> HotKeys = new(); 

    public SmartEventHandler<string> HotKeyPressed { get; set; }

    public HotKeyService(IJSRuntime jsRuntime, ILogger<SmartEventHandler> eventHandlerLogger)
    {
        JsRuntime = jsRuntime;

        HotKeyPressed = new(eventHandlerLogger);
    }

    public async Task RegisterHotkey(string key, string modifier, string action)
    {
        var reference = DotNetObjectReference.Create(this);
        
        if(HotKeys.Any(x => x.Key == key && x.Modifier == modifier))
            return;

        await JsRuntime.InvokeVoidAsync("moonlight.hotkeys.registerHotkey", key, modifier, action, reference);
        
        HotKeys.Add(new()
        {
            Key = key,
            Modifier = modifier,
            Action = action
        });
    }

    public async Task UnregisterHotkey(string key, string modifier)
    {
        var model = HotKeys.FirstOrDefault(x => x.Key == key && x.Modifier == modifier);
        
        if(model == null)
            return;
        
        await JsRuntime.InvokeVoidAsync("moonlight.hotkeys.unregisterHotkey", key, modifier);

        HotKeys.Remove(model);
    }

    [JSInvokable]
    public async void OnHotkeyPressed(string hotKey)
    {
        await HotKeyPressed.Invoke(hotKey);
    }
}