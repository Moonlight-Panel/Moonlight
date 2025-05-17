using System.IO.Compression;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Scripts.Models;

namespace Scripts.Helpers;

public class NupkgHelper
{
    private readonly ILogger<NupkgHelper> Logger;

    public NupkgHelper(ILogger<NupkgHelper> logger)
    {
        Logger = logger;
    }

    public async Task<NupkgManifest?> GetManifest(ZipArchive nugetPackage)
    {
        var nuspecEntry = nugetPackage.Entries.FirstOrDefault(
            x => x.Name.EndsWith(".nuspec")
        );

        if (nuspecEntry == null)
            return null;

        await using var fs = nuspecEntry.Open();
        
        var nuspec = await XDocument.LoadAsync(fs, LoadOptions.None, CancellationToken.None);

        var ns = nuspec.Root!.GetDefaultNamespace();
        var metadata = nuspec.Root!.Element(ns + "metadata")!;

        var id = metadata.Element(ns + "id")!.Value;
        var version = metadata.Element(ns + "version")!.Value;
        var tags = metadata.Element(ns + "tags")!.Value;

        return new NupkgManifest()
        {
            Id = id,
            Version = version,
            Tags = tags.Split(";", StringSplitOptions.RemoveEmptyEntries)
        };
    }
    
    public async Task CleanDependencies(ZipArchive nugetPackage, string filter)
    {
        var nuspecEntry = nugetPackage.Entries.FirstOrDefault(
            x => x.Name.EndsWith(".nuspec")
        );

        if (nuspecEntry == null)
        {
            Logger.LogWarning("No nuspec file to modify found in nuget package");
            return;
        }

        await ModifyXmlInPackage(nugetPackage, nuspecEntry, document =>
        {
            var ns = document.Root!.GetDefaultNamespace();

            return document
                .Descendants(ns + "dependency")
                .Where(x => x.Attribute("id")?.Value.StartsWith(filter) ?? false);
        });
    }

    public async Task RemoveContentFiles(ZipArchive nugetPackage)
    {
        foreach (var entry in nugetPackage.Entries.ToArray())
        {
            if (!entry.FullName.StartsWith("contentFiles") && !entry.FullName.StartsWith("content"))
                continue;

            Logger.LogDebug("Removing content file: {path}", entry.FullName);
            entry.Delete();
        }

        var nuspecFile = nugetPackage
            .Entries
            .FirstOrDefault(x => x.Name.EndsWith(".nuspec"));

        if (nuspecFile == null)
        {
            Logger.LogWarning("Nuspec file missing. Unable to remove content files references from nuspec file");
            return;
        }
        
        await ModifyXmlInPackage(
            nugetPackage,
            nuspecFile,
            document =>
            {
                var ns = document.Root!.GetDefaultNamespace();
                return document.Descendants(ns + "contentFiles");
            }
        );
    }

    public async Task<ZipArchiveEntry> ModifyXmlInPackage(
        ZipArchive nugetPackage,
        ZipArchiveEntry entry,
        Func<XDocument, IEnumerable<XElement>> filter
    )
    {
        var oldPath = entry.FullName;
        await using var oldFs = entry.Open();

        var document = await XDocument.LoadAsync(
            oldFs,
            LoadOptions.None,
            CancellationToken.None
        );

        var itemsToRemove = filter.Invoke(document);
        var items = itemsToRemove.ToArray();

        foreach (var item in items)
            item.Remove();

        oldFs.Close();
        entry.Delete();

        var newEntry = nugetPackage.CreateEntry(oldPath);
        var newFs = newEntry.Open();

        await document.SaveAsync(newFs, SaveOptions.None, CancellationToken.None);

        await newFs.FlushAsync();
        newFs.Close();

        return newEntry;
    }

    public async Task RemoveStaticWebAssets(ZipArchive nugetPackage, string filter)
    {
        var filterWithPath = $"staticwebassets/{filter}";
        
        foreach (var entry in nugetPackage.Entries.ToArray())
        {
            if (!entry.FullName.StartsWith(filterWithPath))
                continue;

            Logger.LogDebug("Removing file: {name}", entry.FullName);
            entry.Delete();
        }

        var buildTargetEntry = nugetPackage.Entries.FirstOrDefault(x =>
            x.FullName == "build/Microsoft.AspNetCore.StaticWebAssets.props"
        );

        if (buildTargetEntry == null)
        {
            Logger.LogWarning("Unable to find Microsoft.AspNetCore.StaticWebAssets.props to remove file references");
            return;
        }

        Logger.LogDebug("Removing file references");

        await ModifyXmlInPackage(nugetPackage, buildTargetEntry,
            document => document
                .Descendants("StaticWebAsset")
                .Where(x =>
                {
                    var relativePath = x.Element("RelativePath")!.Value;
                    return relativePath.StartsWith(filter);
                })
        );
    }

    public async Task AddSourceFiles(ZipArchive nugetPackage, string[] files, Func<string, string> buildPath)
    {
        foreach (var sourceFile in files)
        {
            var path = buildPath.Invoke(sourceFile);

            Logger.LogDebug("Adding additional files as src: {path}", path);

            await using var fs = File.Open(
                sourceFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );

            var entry = nugetPackage.CreateEntry(path);
            await using var entryFs = entry.Open();

            await fs.CopyToAsync(entryFs);
            await entryFs.FlushAsync();

            fs.Close();
            entryFs.Close();
        }
    }
}