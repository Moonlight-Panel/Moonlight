using System.IO.Compression;
using System.Text;

namespace Moonlight.Core.Extensions;

public static class ZipArchiveExtensions
{
    public static async Task AddBinary(this ZipArchive archive, string name, byte[] bytes)
    {
        var entry = archive.CreateEntry(name);
        await using var dataStream = entry.Open();
        
        await dataStream.WriteAsync(bytes);
        await dataStream.FlushAsync();
    }

    public static async Task AddText(this ZipArchive archive, string name, string content)
    {
        var data = Encoding.UTF8.GetBytes(content);
        await archive.AddBinary(name, data);
    }

    public static async Task AddFile(this ZipArchive archive, string name, string path)
    {
        var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        
        var entry = archive.CreateEntry(name);
        await using var dataStream = entry.Open();

        await fs.CopyToAsync(dataStream);
        await dataStream.FlushAsync();
    }
}