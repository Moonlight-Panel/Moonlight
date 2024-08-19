using System.IO.Compression;
using MoonCore.Helpers;
using Moonlight.ApiServer.App.Extensions;
using Moonlight.ApiServer.App.Interfaces;

namespace Moonlight.ApiServer.App.Implementations.Diagnose;

public class LogDiagnoseReporter : IDiagnoseReporter
{
    public async Task GenerateReport(ZipArchive archive, IServiceProvider provider)
    {
        await archive.AddFile("latest-log.txt", PathBuilder.File("storage", "logs", "moonlight.log"));
    }
}