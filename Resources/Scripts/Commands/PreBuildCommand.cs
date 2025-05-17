using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Cocona;
using Microsoft.Extensions.Logging;
using Scripts.Helpers;
using Scripts.Models;

namespace Scripts.Commands;

public class PreBuildCommand
{
    private readonly NupkgHelper NupkgHelper;
    private readonly CsprojHelper CsprojHelper;
    private readonly CodeHelper CodeHelper;
    private readonly ILogger<PreBuildCommand> Logger;

    private const string GeneratedStart = "// MLBUILD Generated Start";
    private const string GeneratedEnd = "// MLBUILD Generated End";
    private const string GeneratedHook = "// MLBUILD_PLUGIN_STARTUP_HERE";

    private readonly string[] ValidTags = ["frontend", "apiserver", "shared"];

    public PreBuildCommand(
        CsprojHelper csprojHelper,
        NupkgHelper nupkgHelper,
        CodeHelper codeHelper,
        ILogger<PreBuildCommand> logger
    )
    {
        CsprojHelper = csprojHelper;
        NupkgHelper = nupkgHelper;
        CodeHelper = codeHelper;
        Logger = logger;
    }

    [Command("prebuild")]
    public async Task Prebuild(
        [Argument] string moonlightDirectory,
        [Argument] string pluginsDirectory
    )
    {
        var projects = await CsprojHelper.FindProjectsInPath(moonlightDirectory, ValidTags);

        var nugetManifests = await GetNugetManifests(pluginsDirectory);

        Logger.LogInformation("Following plugins found:");

        foreach (var manifest in nugetManifests)
        {
            Logger.LogInformation(
                "- {id} ({version}) [{tags}]",
                manifest.Id,
                manifest.Version,
                string.Join(", ", manifest.Tags)
            );
        }

        try
        {
            Logger.LogInformation("Adjusting csproj files");

            foreach (var project in projects)
            {
                var csProjectPath = project.Key;

                await using var fs = File.Open(
                    csProjectPath,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite
                );

                var document = await XDocument.LoadAsync(fs, LoadOptions.None, CancellationToken.None);
                fs.Position = 0;

                var dependenciesToAdd = nugetManifests
                    .Where(x => x.Tags.Any(tag =>
                        project.Value.PackageTags.Contains(tag, StringComparer.InvariantCultureIgnoreCase)))
                    .ToArray();

                await CsprojHelper.CleanDependencies(document, "MoonlightBuildDeps");
                await CsprojHelper.AddDependencies(document, dependenciesToAdd, "MoonlightBuildDeps");

                fs.Position = 0;
                await document.SaveAsync(fs, SaveOptions.None, CancellationToken.None);

                await fs.FlushAsync();
                fs.Close();
            }

            Logger.LogInformation("Restoring projects");
            
            foreach (var csProjectPath in projects.Keys)
                await CsprojHelper.Restore(csProjectPath);

            Logger.LogInformation("Generating plugin startup");

            foreach (var currentTag in ValidTags)
            {
                Logger.LogInformation("Checking for '{currentTag}' projects", currentTag);

                var projectsWithTag = projects
                    .Where(x =>
                        x.Value.PackageTags.Contains(currentTag, StringComparer.InvariantCultureIgnoreCase)
                    )
                    .ToArray();

                foreach (var project in projectsWithTag)
                {
                    var csProjectPath = project.Key;

                    var currentDependencies = nugetManifests
                        .Where(x => x.Tags.Contains(currentTag))
                        .ToArray();

                    var classPaths = await FindStartupClasses(currentDependencies);

                    var code = new StringBuilder();

                    code.AppendLine(GeneratedStart);

                    foreach (var path in classPaths)
                        code.AppendLine($"pluginStartups.Add(new global::{path}());");

                    code.Append(GeneratedEnd);

                    var filesToSearch = Directory.GetFiles(
                        Path.GetDirectoryName(csProjectPath)!,
                        "*.cs",
                        SearchOption.AllDirectories
                    );

                    foreach (var file in filesToSearch)
                    {
                        var content = await File.ReadAllTextAsync(file);

                        if (!content.Contains(GeneratedHook, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        Logger.LogInformation("Injecting generated code to: {path}", Path.GetFullPath(file));

                        content = content.Replace(
                            GeneratedHook,
                            code.ToString(),
                            StringComparison.InvariantCultureIgnoreCase
                        );

                        await File.WriteAllTextAsync(file, content);
                    }
                }
            }
        }
        catch (Exception)
        {
            Logger.LogInformation("An error occured while prebuilding moonlight. Removing csproj modifications");

            foreach (var project in projects)
            {
                await CsprojHelper.CleanDependencies(project.Key, "MoonlightBuildDeps");
                
                var path = Path.GetDirectoryName(project.Key)!;
                await RemoveGeneratedCode(path);
            }

            throw;
        }
    }

    [Command("prebuild-reset")]
    public async Task PrebuildReset(
        [Argument] string moonlightDir
    )
    {
        var projects = await CsprojHelper.FindProjectsInPath(moonlightDir, ValidTags);
        
        Logger.LogInformation("Reverting csproj changes");

        foreach (var project in projects)
        {
            Logger.LogInformation("Removing dependencies: {project}", project.Key);
            await CsprojHelper.CleanDependencies(project.Key, "MoonlightBuildDeps");

            Logger.LogInformation("Removing generated code: {project}", project.Key);
            var path = Path.GetDirectoryName(project.Key)!;
            await RemoveGeneratedCode(path);
        }
    }

    private async Task<NupkgManifest[]> GetNugetManifests(string nugetDir)
    {
        var nugetFiles = Directory.GetFiles(
            nugetDir,
            "*.nupkg",
            SearchOption.AllDirectories
        );

        var manifests = new List<NupkgManifest>();

        foreach (var nugetFilePath in nugetFiles)
        {
            using var nugetPackage = ZipFile.Open(nugetFilePath, ZipArchiveMode.Read);
            var manifest = await NupkgHelper.GetManifest(nugetPackage);

            if (manifest == null)
                continue;

            manifests.Add(manifest);
        }

        return manifests.ToArray();
    }

    private async Task<string[]> FindStartupClasses(NupkgManifest[] dependencies)
    {
        var nugetPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages"
        );

        var filesToScan = dependencies
            .SelectMany(dependency =>
            {
                var dependencySrcPath = Path.Combine(nugetPath, dependency.Id.ToLower(), dependency.Version, "src");

                Logger.LogDebug("Checking {dependencySrcPath}", dependencySrcPath);

                if (!Directory.Exists(dependencySrcPath))
                    return [];

                return Directory.GetFiles(dependencySrcPath, "*.cs", SearchOption.AllDirectories);
            })
            .ToArray();

        return await CodeHelper.FindPluginStartups(filesToScan);
    }

    private async Task RemoveGeneratedCode(string dir)
    {
        var filesToSearch = Directory.GetFiles(
            dir,
            "*.cs",
            SearchOption.AllDirectories
        );

        foreach (var file in filesToSearch)
        {
            var content = await File.ReadAllTextAsync(file);

            if (!content.Contains(GeneratedStart) || !content.Contains(GeneratedEnd))
                continue;

            var startIndex = content.IndexOf(GeneratedStart, StringComparison.InvariantCultureIgnoreCase);
            var endIndex = content.IndexOf(GeneratedEnd, startIndex, StringComparison.InvariantCultureIgnoreCase) +
                           GeneratedEnd.Length;

            var cutOut = content.Substring(startIndex, endIndex - startIndex);

            content = content.Replace(cutOut, GeneratedHook);

            await File.WriteAllTextAsync(file, content);
        }
    }
}