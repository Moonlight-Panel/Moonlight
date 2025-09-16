using System.IO.Compression;
using System.Text;

namespace Moonlight.ApiServer.Extensions;

public static class ZipArchiveExtensions
{
    public static async Task AddBinaryAsync(this ZipArchive archive, string name, byte[] bytes)
    {
        var entry = archive.CreateEntry(name);
        await using var dataStream = entry.Open();
        
        await dataStream.WriteAsync(bytes);
        await dataStream.FlushAsync();
    }

    public static async Task AddTextAsync(this ZipArchive archive, string name, string content)
    {
        var data = Encoding.UTF8.GetBytes(content);
        await archive.AddBinaryAsync(name, data);
    }

    public static async Task AddFileAsync(this ZipArchive archive, string name, string path)
    {
        var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        
        var entry = archive.CreateEntry(name);
        await using var dataStream = entry.Open();

        await fs.CopyToAsync(dataStream);
        await dataStream.FlushAsync();
    }
}