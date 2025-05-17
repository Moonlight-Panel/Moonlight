using System.IO.Compression;
using System.Xml.Linq;
using Cocona;
using Microsoft.Extensions.Logging;
using Scripts.Helpers;

namespace Scripts.Commands;

public class PackCommand
{
    private readonly string TmpDir = "/tmp/mlbuild";
    private readonly ILogger<PackCommand> Logger;
    private readonly CsprojHelper CsprojHelper;
    private readonly NupkgHelper NupkgHelper;

    private readonly string[] ValidTags = ["apiserver", "frontend", "shared"];

    public PackCommand(
        ILogger<PackCommand> logger,
        CsprojHelper csprojHelper,
        NupkgHelper nupkgHelper
    )
    {
        CsprojHelper = csprojHelper;
        NupkgHelper = nupkgHelper;
        Logger = logger;
    }

    [Command("pack", Description = "Packs the specified folder/solution into nuget packages")]
    public async Task Pack(
        [Argument] string solutionDirectory,
        [Argument] string outputLocation,
        [Option] string buildConfiguration = "Debug"
    )
    {
        if (!Directory.Exists(solutionDirectory))
        {
            Logger.LogError("The specified solution directory does not exist");
            return;
        }

        if (!Directory.Exists(outputLocation))
            Directory.CreateDirectory(outputLocation);

        if (Directory.Exists(TmpDir))
            Directory.Delete(TmpDir, true);

        Directory.CreateDirectory(TmpDir);

        // Find the project files
        Logger.LogInformation("Searching for projects inside the specified folder");

        var projects = await CsprojHelper.FindProjectsInPath(solutionDirectory, ValidTags);

        // Show the user
        Logger.LogInformation("Found {count} project(s) to check:", projects.Count);

        foreach (var path in projects.Keys)
            Logger.LogInformation("- {path}", Path.GetFullPath(path));

        // Filter out project files which have specific tags specified
        Logger.LogInformation("Filtering projects by tags");

        var apiServerProjects = projects
            .Where(x => x.Value.PackageTags.Contains("apiserver", StringComparer.InvariantCultureIgnoreCase))
            .ToArray();

        var frontendProjects = projects
            .Where(x => x.Value.PackageTags.Contains("frontend", StringComparer.InvariantCultureIgnoreCase))
            .ToArray();

        var sharedProjects = projects
            .Where(x => x.Value.PackageTags.Contains("shared", StringComparer.InvariantCultureIgnoreCase))
            .ToArray();

        Logger.LogInformation(
            "Found {apiServerCount} api server project(s), {frontendCount} frontend project(s) and {sharedCount} shared project(s)",
            apiServerProjects.Length,
            frontendProjects.Length,
            sharedProjects.Length
        );

        // Now build all these projects so we can pack them
        Logger.LogInformation("Building and packing api server project(s)");

        foreach (var apiServerProject in apiServerProjects)
        {
            var csProjectFile = apiServerProject.Key;
            var manifest = apiServerProject.Value;

            await CsprojHelper.Build(
                csProjectFile,
                buildConfiguration
            );

            var nugetFilePath = await CsprojHelper.Pack(
                csProjectFile,
                TmpDir,
                buildConfiguration
            );

            var nugetPackage = ZipFile.Open(
                nugetFilePath,
                ZipArchiveMode.Update
            );

            await NupkgHelper.RemoveContentFiles(nugetPackage);

            // We don't want to clean moonlight references when we are packing moonlight,
            // as it would remove references to its own shared project

            if (!manifest.PackageId.StartsWith("Moonlight."))
                await NupkgHelper.CleanDependencies(nugetPackage, "Moonlight.");

            Logger.LogInformation("Finishing package and copying to output directory");

            nugetPackage.Dispose();

            File.Move(
                nugetFilePath,
                Path.Combine(outputLocation, Path.GetFileName(nugetFilePath)),
                true
            );
        }

        Logger.LogInformation("Building and packing frontend projects");

        foreach (var frontendProject in frontendProjects)
        {
            var csProjectFile = frontendProject.Key;
            var manifest = frontendProject.Value;

            await CsprojHelper.Build(
                csProjectFile,
                buildConfiguration
            );

            var nugetFilePath = await CsprojHelper.Pack(
                csProjectFile,
                TmpDir,
                buildConfiguration
            );

            var nugetPackage = ZipFile.Open(
                nugetFilePath,
                ZipArchiveMode.Update
            );

            await NupkgHelper.RemoveStaticWebAssets(nugetPackage, "_framework");
            await NupkgHelper.RemoveStaticWebAssets(nugetPackage, "css/style.min.css");
            await NupkgHelper.RemoveContentFiles(nugetPackage);

            // We don't want to clean moonlight references when we are packing moonlight,
            // as it would remove references to its own shared project

            if (!manifest.PackageId.StartsWith("Moonlight."))
                await NupkgHelper.CleanDependencies(nugetPackage, "Moonlight.");


            // Pack razor and html files into src folder
            var additionalSrcFiles = new List<string>();
            var basePath = Path.GetDirectoryName(csProjectFile)!;

            additionalSrcFiles.AddRange(
                Directory.GetFiles(basePath, "*.razor", SearchOption.AllDirectories)
            );

            additionalSrcFiles.AddRange(
                Directory.GetFiles(basePath, "index.html", SearchOption.AllDirectories)
            );

            await NupkgHelper.AddSourceFiles(
                nugetPackage,
                additionalSrcFiles.ToArray(),
                file => "src/" + file.Replace(basePath, "").Trim('/')
            );

            Logger.LogInformation("Finishing package and copying to output directory");

            nugetPackage.Dispose();

            File.Move(
                nugetFilePath,
                Path.Combine(outputLocation, Path.GetFileName(nugetFilePath)),
                true
            );
        }

        Logger.LogInformation("Building and packing shared projects");

        foreach (var sharedProject in sharedProjects)
        {
            var csProjectFile = sharedProject.Key;
            var manifest = sharedProject.Value;

            await CsprojHelper.Build(
                csProjectFile,
                buildConfiguration
            );

            var nugetFilePath = await CsprojHelper.Pack(
                csProjectFile,
                TmpDir,
                buildConfiguration
            );
            
            var nugetPackage = ZipFile.Open(
                nugetFilePath,
                ZipArchiveMode.Update
            );

            // We don't want to clean moonlight references when we are packing moonlight,
            // as it would remove references to its own shared project

            if (!manifest.PackageId.StartsWith("Moonlight."))
                await NupkgHelper.CleanDependencies(nugetPackage, "Moonlight.");
            
            nugetPackage.Dispose();

            File.Move(
                nugetFilePath,
                Path.Combine(outputLocation, Path.GetFileName(nugetFilePath)),
                true
            );
        }
    }
}