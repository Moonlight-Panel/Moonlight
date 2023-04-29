using System.Text;
using Microsoft.JSInterop;

namespace Moonlight.App.Services;

public class FileDownloadService
{
    private readonly IJSRuntime JSRuntime;
    
    public FileDownloadService(IJSRuntime jsRuntime)
    {
        JSRuntime = jsRuntime;
    }

    public async Task DownloadStream(string fileName, Stream stream)
    {
        using var streamRef = new DotNetStreamReference(stream);

        await JSRuntime.InvokeVoidAsync("moonlight.downloads.downloadStream", fileName, streamRef);
    }

    public async Task DownloadBytes(string fileName, byte[] bytes)
    {
        var ms = new MemoryStream(bytes);
        
        await DownloadStream(fileName, ms);
    }

    public async Task DownloadString(string fileName, string content)
    {
        await DownloadBytes(fileName, Encoding.UTF8.GetBytes(content));
    }
}