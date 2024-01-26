using System.IO.Compression;
using System.Text;

namespace Moonlight.Core.Extensions;

public static class ZipArchiveExtension
{
    public static async Task AddFromText(this ZipArchive archive, string entryName, string content)
    {
        using var memoryStream = new MemoryStream();
        await memoryStream.WriteAsync(Encoding.UTF8.GetBytes(content));
        await memoryStream.FlushAsync();

        await archive.AddFromStream(entryName, memoryStream);
    }

    public static async Task AddFromStream(this ZipArchive archive, string entryName, Stream dataStream)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
        await using var stream = entry.Open();

        dataStream.Position = 0;
        await dataStream.CopyToAsync(stream);
        await stream.FlushAsync();
    }
}