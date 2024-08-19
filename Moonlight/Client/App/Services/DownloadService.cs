using System.Text;
using Microsoft.JSInterop;
using MoonCore.Attributes;

namespace Moonlight.Client.App.Services;

[Scoped]
public class DownloadService
{
    private readonly IJSRuntime JsRuntime;
    
    public DownloadService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task DownloadStream(string fileName, Stream stream)
    {
        using var streamRef = new DotNetStreamReference(stream);
        await JsRuntime.InvokeVoidAsync("moonlight.misc.download", fileName, streamRef);
    }

    public async Task DownloadBytes(string fileName, byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        
        await DownloadStream(fileName, ms);
        
        ms.Close();
    }

    public async Task DownloadString(string fileName, string content) =>
        await DownloadBytes(fileName, Encoding.UTF8.GetBytes(content));
}