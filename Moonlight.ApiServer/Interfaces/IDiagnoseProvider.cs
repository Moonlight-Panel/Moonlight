using System.IO.Compression;
using Moonlight.ApiServer.Models.Diagnose;

namespace Moonlight.ApiServer.Interfaces;

public interface IDiagnoseProvider
{
    public Task ModifyZipArchive(ZipArchive archive);
}