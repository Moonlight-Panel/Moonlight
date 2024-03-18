using System.IO.Compression;

namespace Moonlight.Core.Interfaces;

public interface IDiagnoseAction
{
    public Task GenerateReport(ZipArchive archive, IServiceProvider serviceProvider);
}