using System.IO.Compression;
using Moonlight.ApiServer.Extensions;
using Moonlight.ApiServer.Interfaces;

namespace Moonlight.ApiServer.Implementations.Diagnose;

public class LogsDiagnoseProvider : IDiagnoseProvider
{
    public async Task ModifyZipArchiveAsync(ZipArchive archive)
    {
        var path = Path.Combine("storage", "logs", "moonlight.log");

        if (File.Exists(path))
        {
            var logsContent = await File.ReadAllTextAsync(path);
            await archive.AddTextAsync("logs.txt", logsContent);
        }
        else
            await archive.AddTextAsync("logs.txt", "Logs file moonlight.log has not been found");
    }
}