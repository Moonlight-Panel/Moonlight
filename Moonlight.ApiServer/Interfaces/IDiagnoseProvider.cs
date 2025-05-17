using System.IO.Compression;

namespace Moonlight.ApiServer.Interfaces;

public interface IDiagnoseProvider
{
    public Task ModifyZipArchive(ZipArchive archive);
}