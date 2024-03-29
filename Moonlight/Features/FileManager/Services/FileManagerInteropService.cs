using Microsoft.JSInterop;
using MoonCore.Attributes;
using MoonCore.Helpers;

namespace Moonlight.Features.FileManager.Services;

[Scoped]
public class FileManagerInteropService
{
    private readonly IJSRuntime JsRuntime;

    public SmartEventHandler OnUploadStateChanged { get; set; } = new();

    public FileManagerInteropService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task InitDropzone(string id, string urlId)
    {
        var reference = DotNetObjectReference.Create(this);
        await JsRuntime.InvokeVoidAsync("filemanager.dropzone.init", id, urlId, reference);
    }
    
    public async Task InitFileSelect(string id, string urlId)
    {
        var reference = DotNetObjectReference.Create(this);
        await JsRuntime.InvokeVoidAsync("filemanager.fileselect.init", id, urlId, reference);
    }

    public async Task UpdateUrl(string urlId, string url)
    {
        await JsRuntime.InvokeVoidAsync("filemanager.updateUrl", urlId, url);
    }

    [JSInvokable]
    public async Task UpdateStatus()
    {
        await OnUploadStateChanged.Invoke();
    }
}