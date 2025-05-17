using System.Collections.Frozen;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Scripts.Models;

namespace Scripts.Helpers;

public class CsprojHelper
{
    private readonly ILogger<CsprojHelper> Logger;
    private readonly CommandHelper CommandHelper;

    public CsprojHelper(ILogger<CsprojHelper> logger, CommandHelper commandHelper)
    {
        Logger = logger;
        CommandHelper = commandHelper;
    }

    #region Add dependencies

    public async Task AddDependencies(string path, NupkgManifest[] dependencies, string label)
    {
        await using var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        fs.Position = 0;

        await AddDependencies(fs, dependencies, label);

        await fs.FlushAsync();
        fs.Close();
    }

    public async Task AddDependencies(Stream stream, NupkgManifest[] dependencies, string label)
    {
        var xmlDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

        await AddDependencies(xmlDocument, dependencies, label);

        stream.Position = 0;
        await xmlDocument.SaveAsync(stream, SaveOptions.DisableFormatting, CancellationToken.None);
    }

    public Task AddDependencies(XDocument document, NupkgManifest[] dependencies, string label)
    {
        var project = document.Element("Project")!;

        var itemGroup = new XElement("ItemGroup");
        itemGroup.SetAttributeValue("Label", label);

        foreach (var dependency in dependencies)
        {
            var depElement = new XElement("PackageReference");
            depElement.SetAttributeValue("Include", dependency.Id);
            depElement.SetAttributeValue("Version", dependency.Version);

            itemGroup.Add(depElement);
        }

        project.Add(itemGroup);

        return Task.CompletedTask;
    }

    #endregion

    #region Clean dependencies

    public async Task CleanDependencies(string path, string label)
    {
        var document = XDocument.Load(path, LoadOptions.None);

        await CleanDependencies(document, label);

        document.Save(path, SaveOptions.DisableFormatting);
    }

    public async Task CleanDependencies(Stream stream, string label)
    {
        var xmlDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

        await CleanDependencies(xmlDocument, label);

        stream.Position = 0;
        await xmlDocument.SaveAsync(stream, SaveOptions.DisableFormatting, CancellationToken.None);
    }

    public Task CleanDependencies(XDocument document, string label)
    {
        var itemGroupsToRemove = document
            .Descendants("ItemGroup")
            .Where(x => x.Attribute("Label")?.Value.Contains(label) ?? false)
            .ToArray();

        itemGroupsToRemove.Remove();

        return Task.CompletedTask;
    }

    #endregion

    #region Read

    public async Task<CsprojManifest> Read(string path)
    {
        await using var fileStream = File.Open(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite
        );

        var manifest = await Read(fileStream);

        fileStream.Close();

        return manifest;
    }

    public async Task<CsprojManifest> Read(Stream stream)
    {
        var xmlDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        return await Read(xmlDocument);
    }

    public Task<CsprojManifest> Read(XDocument document)
    {
        var manifest = new CsprojManifest();

        var ns = document.Root!.GetDefaultNamespace();

        manifest.IsPackable = document
            .Descendants(ns + "IsPackable")
            .FirstOrDefault()?.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;

        manifest.PackageId = document
            .Descendants(ns + "PackageId")
            .FirstOrDefault()?.Value ?? "N/A";

        manifest.Version = document
            .Descendants(ns + "Version")
            .FirstOrDefault()?.Value ?? "N/A";

        manifest.PackageTags = document
            .Descendants(ns + "PackageTags")
            .FirstOrDefault()?.Value
            .Split(";", StringSplitOptions.RemoveEmptyEntries) ?? [];

        return Task.FromResult(manifest);
    }

    #endregion

    public async Task Restore(string path)
    {
        var basePath = Path.GetFullPath(Path.GetDirectoryName(path)!);
        var fileName = Path.GetFileName(path);

        Logger.LogInformation("Restore: {basePath} - {fileName}", basePath, fileName);

        await CommandHelper.Run(
            "/usr/bin/dotnet",
            $"restore {fileName}",
            basePath
        );
    }

    public async Task Build(string file, string configuration)
    {
        var basePath = Path.GetFullPath(Path.GetDirectoryName(file)!);
        var fileName = Path.GetFileName(file);

        await CommandHelper.Run(
            "/usr/bin/dotnet",
            $"build {fileName} --configuration {configuration}",
            basePath
        );
    }

    public async Task<string> Pack(string file, string output, string configuration)
    {
        var basePath = Path.GetFullPath(Path.GetDirectoryName(file)!);
        var fileName = Path.GetFileName(file);

        await CommandHelper.Run(
            "/usr/bin/dotnet",
            $"pack {fileName} --output {output} --configuration {configuration}",
            basePath
        );

        var nugetFilesPaths = Directory.GetFiles(
            output,
            "*.nupkg",
            SearchOption.TopDirectoryOnly
        );

        if (nugetFilesPaths.Length == 0)
            throw new Exception("No nuget packages were built");

        if (nugetFilesPaths.Length > 1)
            throw new Exception("More than one nuget package has been built");

        return nugetFilesPaths.First();
    }

    public async Task<FrozenDictionary<string, CsprojManifest>> FindProjectsInPath(string path, string[] validTags)
    {
        var projectFiles = Directory.GetFiles(
            path,
            "*.csproj",
            SearchOption.AllDirectories
        );

        var projects = new Dictionary<string, CsprojManifest>();

        foreach (var projectFile in projectFiles)
        {
            var manifest = await Read(projectFile);

            // Ignore all projects which have no matching tags
            if (!manifest.PackageTags.Any(projectTag =>
                    validTags.Contains(projectTag, StringComparer.InvariantCultureIgnoreCase)))
                continue;

            projects.Add(projectFile, manifest);
        }

        return projects.ToFrozenDictionary();
    }
}