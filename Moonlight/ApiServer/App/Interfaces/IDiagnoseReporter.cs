using System.IO.Compression;

namespace Moonlight.ApiServer.App.Interfaces;

public interface IDiagnoseReporter
{
    public Task GenerateReport(ZipArchive archive, IServiceProvider provider);
}