using Microsoft.JSInterop;
using MoonCore.Attributes;
using MoonCore.Blazor.Extensions;
using MoonCore.Helpers;

namespace Moonlight.Features.FileManager.Services;

[Scoped]
public class FileManagerInteropService
{
    private readonly IJSRuntime JsRuntime;

    public SmartEventHandler OnUploadStateChanged { get; set; }

    public FileManagerInteropService(IJSRuntime jsRuntime, ILogger<SmartEventHandler> eventHandlerLogger)
    {
        JsRuntime = jsRuntime;

        OnUploadStateChanged = new(eventHandlerLogger);
    }

    public async Task InitDropzone(string id, string urlId)
    {
        var reference = DotNetObjectReference.Create(this);
        await JsRuntime.InvokeVoidAsyncHandled("filemanager.dropzone.init", id, urlId, reference);
    }
    
    public async Task InitFileSelect(string id, string urlId)
    {
        var reference = DotNetObjectReference.Create(this);
        await JsRuntime.InvokeVoidAsyncHandled("filemanager.fileselect.init", id, urlId, reference);
    }

    public async Task UpdateUrl(string urlId, string url)
    {
        await JsRuntime.InvokeVoidAsyncHandled("filemanager.updateUrl", urlId, url);
    }

    [JSInvokable]
    public async Task UpdateStatus()
    {
        await OnUploadStateChanged.Invoke();
    }
}