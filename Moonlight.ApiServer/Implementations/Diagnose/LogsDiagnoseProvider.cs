using System.IO.Compression;
using System.Text;
using MoonCore.Helpers;
using Moonlight.ApiServer.Extensions;
using Moonlight.ApiServer.Interfaces;

namespace Moonlight.ApiServer.Implementations.Diagnose;

public class LogsDiagnoseProvider : IDiagnoseProvider
{
    public async Task ModifyZipArchive(ZipArchive archive)
    {

            var logs = await File.ReadAllTextAsync(PathBuilder.File("storage", "logs", "latest.log"));

            if (string.IsNullOrEmpty(logs))
            {
                await archive.AddText("logs.txt", "Could not read the logs");
                return;
            }
            
            await archive.AddText("logs.txt", logs);
    }
}