using Microsoft.JSInterop;
using MoonCore.Attributes;

namespace Moonlight.Features.FileManager.Services;

[Scoped]
public class EditorService
{
    private readonly IJSRuntime JsRuntime;

    public EditorService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task Create(string mount, string theme = "one_dark", string mode = "text", string initialContent = "",
        int lines = 30, int fontSize = 15)
    {
        await JsRuntime.InvokeVoidAsync(
            "moonlight.editor.create",
            mount,
            theme,
            mode,
            initialContent,
            lines,
            fontSize
        );
    }

    public async Task SetValue(string content) => await JsRuntime.InvokeVoidAsync("moonlight.editor.setValue", content);

    public async Task<string> GetValue() => await JsRuntime.InvokeAsync<string>("moonlight.editor.getValue");

    public async Task SetMode(string mode) => await JsRuntime.InvokeVoidAsync("moonlight.editor.setMode", mode);
}