using System.IO.Compression;
using Moonlight.ApiServer.Extensions;
using Moonlight.ApiServer.Interfaces;

namespace Moonlight.ApiServer.Implementations.Diagnose;

public class LogsDiagnoseProvider : IDiagnoseProvider
{
    public async Task ModifyZipArchive(ZipArchive archive)
    {
        var path = Path.Combine("storage", "logs", "latest.log");

        if (!File.Exists(path))
        {
            await archive.AddText("logs.txt", "Logs file latest.log has not been found");
            return;
        }
        
        var logsContent = await File.ReadAllTextAsync(path);
        await archive.AddText("logs.txt", logsContent);
    }
}