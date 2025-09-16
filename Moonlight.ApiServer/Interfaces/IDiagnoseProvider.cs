using System.IO.Compression;

namespace Moonlight.ApiServer.Interfaces;

public interface IDiagnoseProvider
{
    public Task ModifyZipArchiveAsync(ZipArchive archive);
}