using Moonlight.ApiServer.Interfaces;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using Moonlight.Shared.Http.Responses.Admin.Sys;

namespace Moonlight.ApiServer.Services;

[Scoped]
public class DiagnoseService
{
    private readonly IEnumerable<IDiagnoseProvider> DiagnoseProviders;
    private readonly ILogger<DiagnoseService> Logger;

    public DiagnoseService(
        IEnumerable<IDiagnoseProvider> diagnoseProviders,
        ILogger<DiagnoseService> logger
    )
    {
        DiagnoseProviders = diagnoseProviders;
        Logger = logger;
    }

    public Task<DiagnoseProvideResponse[]> GetProvidersAsync()
    {
        var availableProviders = new List<DiagnoseProvideResponse>();

        foreach (var diagnoseProvider in DiagnoseProviders)
        {
            var name = diagnoseProvider.GetType().Name;

            var type = diagnoseProvider.GetType().FullName;

            // The type name is null if the type is a generic type, unlikely, but still could happen
            if (type == null)
                continue;

            availableProviders.Add(new DiagnoseProvideResponse()
            {
                Name = name,
                Type = type
            });
        }

        return Task.FromResult(
            availableProviders.ToArray()
        );
    }

    public async Task<MemoryStream> GenerateDiagnoseAsync(string[] requestedProviders)
    {
        IDiagnoseProvider[] providers;

        if (requestedProviders.Length == 0)
            providers = DiagnoseProviders.ToArray();
        else
        {
            var foundProviders = new List<IDiagnoseProvider>();

            foreach (var requestedProvider in requestedProviders)
            {
                var provider = DiagnoseProviders.FirstOrDefault(x => x.GetType().FullName == requestedProvider);

                if (provider == null)
                    continue;

                foundProviders.Add(provider);
            }

            providers = foundProviders.ToArray();
        }

        try
        {
            var outputStream = new MemoryStream();
            var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true);

            foreach (var provider in providers)
            {
                await provider.ModifyZipArchiveAsync(zipArchive);
            }
            
            zipArchive.Dispose();

            outputStream.Position = 0;
            return outputStream;
        }
        catch (Exception e)
        {
            Logger.LogError("An unhandled error occured while generated the diagnose file: {e}", e);

            throw new HttpApiException("An unhandled error occured while generating the diagnose file", 500);
        }
    }
}