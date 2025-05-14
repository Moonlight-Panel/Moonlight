using MoonCore.Helpers;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models.Diagnose;
using System.IO.Compression;
using System.Text;
using MoonCore.Attributes;
using Moonlight.Shared.Http.Responses.Admin.Sys;
using Moonlight.Shared.Misc;

namespace Moonlight.ApiServer.Services;

[Scoped]
public class DiagnoseService
{
    private readonly IEnumerable<IDiagnoseProvider> DiagnoseProviders;

    public DiagnoseService(IEnumerable<IDiagnoseProvider> diagnoseProviders)
    {
        DiagnoseProviders = diagnoseProviders;
    }

    public async Task<DiagnoseProvideResponse[]> GetAvailable()
    {
        var availableProviders = new List<DiagnoseProvideResponse>();

        foreach (var diagnoseProvider in DiagnoseProviders)
        {
            var name = diagnoseProvider.GetType().Name;

            var type = diagnoseProvider.GetType().FullName;

            // The type name is null if the type is a generic type, unlikely, but still could happen
            if (type != null)
                availableProviders.Add(new DiagnoseProvideResponse()
                {
                    Name = name,
                    Type = type
                });
        }

        return availableProviders.ToArray();
    }

    public async Task GenerateDiagnose(Stream outputStream, string[]? requestedProviders)
    {
        IDiagnoseProvider[] providers;
        
        if (requestedProviders != null && requestedProviders.Length > 0)
        {
            var requesteDiagnoseProviders = new List<IDiagnoseProvider>();

            foreach (var requestedProvider in requestedProviders)
            {
                var provider = DiagnoseProviders.FirstOrDefault(x => x.GetType().FullName == requestedProvider);

                if (provider != null)
                    requesteDiagnoseProviders.Add(provider);
            }

            providers = requesteDiagnoseProviders.ToArray();
        }
        else
        {
            providers = DiagnoseProviders.ToArray();
        }

        try
        {
            using (var zip = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var provider in providers)
                {
                    await provider.ModifyZipArchive(zip);
                }
            }

            outputStream.Seek(0, SeekOrigin.Begin);
        }
        catch (Exception ex)
        {
            throw new Exception($"An unknown error occured while building the Diagnose: {ex.Message}");
        }
    }
}