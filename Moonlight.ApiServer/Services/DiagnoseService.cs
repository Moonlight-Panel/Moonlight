using MoonCore.Helpers;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models.Diagnose;
using System.IO.Compression;
using MoonCore.Attributes;

namespace Moonlight.ApiServer.Services;

[Scoped]
public class DiagnoseService
{

    private readonly IEnumerable<IDiagnoseProvider> DiagnoseProviders;

    public DiagnoseService(IEnumerable<IDiagnoseProvider> diagnoseProviders)
    {
        DiagnoseProviders = diagnoseProviders;
    }

    public async Task GenerateDiagnose(Stream outputStream)
    {
        var tasks = DiagnoseProviders
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

        var directoryPath = path != null ?  string.Join("/", path, directory.Name) : directory.Name;
        
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

        var filePath = path != null ?  string.Join("/", path, file.Name) : file.Name;

        var bytes = file.GetContent();

        return new KeyValuePair<string, byte[]>(filePath, bytes);
    }
}