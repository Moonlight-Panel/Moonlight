using System.Text;
using Microsoft.JSInterop;

namespace Moonlight.App.Services.Interop;

public class FileDownloadService
{
    private readonly IJSRuntime JsRuntime;
    
    public FileDownloadService(IJSRuntime jsRuntime)
    {
        JsRuntime = jsRuntime;
    }

    public async Task DownloadStream(string fileName, Stream stream)
    {
        using var streamRef = new DotNetStreamReference(stream);

        await JsRuntime.InvokeVoidAsync("moonlight.utils.download", fileName, streamRef);
    }

    public async Task DownloadBytes(string fileName, byte[] bytes)
    {
        var ms = new MemoryStream(bytes);
        
        await DownloadStream(fileName, ms);
        
        ms.Close();
        await ms.DisposeAsync();
    }

    public async Task DownloadString(string fileName, string content) =>
        await DownloadBytes(fileName, Encoding.UTF8.GetBytes(content));
}