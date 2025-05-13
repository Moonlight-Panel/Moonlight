using MoonCore.Helpers;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models.Diagnose;
using System.IO.Compression;
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

    public async Task<DiagnoseProvider[]> GetAvailable()
    {
        var availableProviders = new List<DiagnoseProvider>();

        foreach (var diagnoseProvider in DiagnoseProviders)
        {
            var name = diagnoseProvider.GetType().Name;

            var type = diagnoseProvider.GetType().FullName;

            // The type name is null if the type is a generic type, unlikely, but still could happen
            if (type != null)
                availableProviders.Add(new DiagnoseProvider()
                {
                    Name = name,
                    Type = type
                });
        }

        return availableProviders.ToArray();
    }

    public async Task GenerateDiagnose(Stream outputStream, string[]? requestedProviders)
    {
        List<IDiagnoseProvider> providers;
        
        if (requestedProviders != null && requestedProviders.Length > 0)
        {
            providers = new List<IDiagnoseProvider>();

            foreach (var requestedProvider in requestedProviders)
            {
                var provider = DiagnoseProviders.FirstOrDefault(x => x.GetType().FullName == requestedProvider);

                if (provider != null)
                    providers.Add(provider);
            }
        }
        else
        {
            providers = DiagnoseProviders.ToList();
        }


        var tasks = providers
            .SelectMany(x => x.GetFiles())
            .Select(async x => await ProcessDiagnoseEntry(null, x));


        var fileEntries = (await Task.WhenAll(tasks))
            .SelectMany(x => x)
            .ToDictionary();


        try
        {
            using (var zip = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var kv in fileEntries)
                {
                    string entryName = kv.Key.Replace('\\', '/');
                    byte[] data = kv.Value;

                    // Optionally pick CompressionLevel.Optimal or NoCompression
                    var entry = zip.CreateEntry(entryName, CompressionLevel.Fastest);

                    using (var entryStream = entry.Open())
                    using (var ms = new MemoryStream(data))
                    {
                        await ms.CopyToAsync(entryStream);
                    }
                }
            }

            outputStream.Seek(0, SeekOrigin.Begin);
        }
        catch (Exception ex)
        {
            throw new Exception($"An unknown error occured while building the Diagnose: {ex.Message}");
        }
    }

    private async Task<Dictionary<string, byte[]>> ProcessDiagnoseEntry(string? path, DiagnoseEntry entry)
    {
        var fileEntries = new Dictionary<string, byte[]>();

        switch (entry)
        {
            case DiagnoseDirectory diagnoseDirectory:
            {
                var files = await ProcessDiagnoseDirectory(path, diagnoseDirectory);

                foreach (var file in files)
                {
                    fileEntries.Add(file.Key, file.Value);
                }

                break;
            }
            case DiagnoseFile diagnoseFile:
            {
                var file = await ProcessDiagnoseFile(path, diagnoseFile);

                fileEntries.Add(file.Key, file.Value);

                break;
            }
        }

        return fileEntries;
    }

    private async Task<Dictionary<string, byte[]>> ProcessDiagnoseDirectory(string? path, DiagnoseDirectory directory)
    {
        var result = new Dictionary<string, byte[]>();

        var directoryPath = path != null ? string.Join("/", path, directory.Name) : directory.Name;

        foreach (var entry in directory.Children)
        {
            var files = await ProcessDiagnoseEntry(directoryPath, entry);

            foreach (var file in files)
            {
                result.Add(file.Key, file.Value);
            }
        }

        return result;
    }

    private async Task<KeyValuePair<string, byte[]>> ProcessDiagnoseFile(string? path, DiagnoseFile file)
    {
        var filePath = path != null ? string.Join("/", path, file.Name) : file.Name;

        var bytes = file.GetContent();

        return new KeyValuePair<string, byte[]>(filePath, bytes);
    }
}