using System.IO.Compression;
using System.Xml.Linq;
using Cocona;
using Scripts.Helpers;

namespace Scripts.Commands;

public class PackCommand
{
    private readonly CommandHelper CommandHelper;
    private readonly string TmpDir = "/tmp/mlbuild";

    public PackCommand(CommandHelper commandHelper)
    {
        CommandHelper = commandHelper;
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
            Console.WriteLine("The specified solution directory does not exist");
            return;
        }

        if (!Directory.Exists(outputLocation))
            Directory.CreateDirectory(outputLocation);

        if (Directory.Exists(TmpDir))
            Directory.Delete(TmpDir, true);

        Directory.CreateDirectory(TmpDir);

        // Find the project files
        Console.WriteLine("Searching for projects inside the specified folder");
        var csProjFiles = Directory.GetFiles(solutionDirectory, "*csproj", SearchOption.AllDirectories);

        // Show the user
        Console.WriteLine($"Found {csProjFiles.Length} project(s) to check:");

        foreach (var csProjFile in csProjFiles)
            Console.WriteLine($"- {Path.GetFullPath(csProjFile)}");

        // Filter out project files which have specific tags specified
        Console.WriteLine("Filtering projects by tags");

        List<string> apiServerProjects = [];
        List<string> frontendProjects = [];
        List<string> sharedProjects = [];

        foreach (var csProjFile in csProjFiles)
        {
            await using var fs = File.Open(
                csProjFile,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            );

            var document = await XDocument.LoadAsync(fs, LoadOptions.None, CancellationToken.None);
            fs.Close();

            // Search for tag definitions
            var packageTagsElements = document.Descendants("PackageTags").ToArray();

            if (packageTagsElements.Length == 0)
            {
                Console.WriteLine($"No package tags found in {Path.GetFullPath(csProjFile)}. Skipping it when packing");
                continue;
            }

            var packageTags = packageTagsElements.First().Value;

            if (packageTags.Contains("apiserver", StringComparison.InvariantCultureIgnoreCase))
                apiServerProjects.Add(csProjFile);

            if (packageTags.Contains("frontend", StringComparison.InvariantCultureIgnoreCase))
                frontendProjects.Add(csProjFile);

            if (packageTags.Contains("shared", StringComparison.InvariantCultureIgnoreCase))
                sharedProjects.Add(csProjFile);
        }

        Console.WriteLine(
            $"Found {apiServerProjects.Count} api server project(s), {frontendProjects.Count} frontend project(s) and {sharedProjects.Count} shared project(s)");

        // Now build all these projects so we can pack them
        Console.WriteLine("Building and packing api server project(s)");

        foreach (var apiServerProject in apiServerProjects)
        {
            await BuildProject(
                apiServerProject,
                buildConfiguration
            );

            var nugetFilePath = await PackProject(
                apiServerProject,
                TmpDir,
                buildConfiguration
            );

            var nugetPackage = ZipFile.Open(
                nugetFilePath,
                ZipArchiveMode.Update
            );

            await RemoveContentFiles(nugetPackage);

            await CleanDependencies(nugetPackage);
            
            Console.WriteLine("Finishing package and copying to output directory");

            nugetPackage.Dispose();
            File.Move(nugetFilePath, Path.Combine(outputLocation, Path.GetFileName(nugetFilePath)), true);
        }

        Console.WriteLine("Building and packing frontend projects");

        foreach (var frontendProject in frontendProjects)
        {
            await BuildProject(
                frontendProject,
                buildConfiguration
            );

            var nugetFilePath = await PackProject(
                frontendProject,
                TmpDir,
                buildConfiguration
            );

            var nugetPackage = ZipFile.Open(
                nugetFilePath,
                ZipArchiveMode.Update
            );

            foreach (var entry in nugetPackage.Entries.ToArray())
            {
                if (!entry.FullName.StartsWith("staticwebassets/_framework"))
                    continue;

                Console.WriteLine($"Removing framework file: {entry.FullName}");
                entry.Delete();
            }

            var buildTargetEntry = nugetPackage.Entries.FirstOrDefault(x =>
                x.FullName == "build/Microsoft.AspNetCore.StaticWebAssets.props"
            );

            if (buildTargetEntry != null)
            {
                Console.WriteLine("Removing framework file references");

                await ModifyXmlInPackage(nugetPackage, buildTargetEntry,
                    document => document
                        .Descendants("StaticWebAsset")
                        .Where(x =>
                        {
                            var relativePath = x.Element("RelativePath")!.Value;

                            if (relativePath.StartsWith("_framework"))
                                return true;

                            if (relativePath.StartsWith("css/style.min.css"))
                                return true;

                            return false;
                        })
                );
            }

            await CleanDependencies(nugetPackage);

            await RemoveContentFiles(nugetPackage);

            // Pack razor and html files into src folder
            var additionalSrcFiles = new List<string>();
            var basePath = Path.GetDirectoryName(frontendProject)!;

            additionalSrcFiles.AddRange(
                Directory.GetFiles(basePath, "*.razor", SearchOption.AllDirectories)
            );

            additionalSrcFiles.AddRange(
                Directory.GetFiles(basePath, "index.html", SearchOption.AllDirectories)
            );

            foreach (var additionalSrcFile in additionalSrcFiles)
            {
                var relativePath = "src/" + additionalSrcFile.Replace(basePath, "").Trim('/');

                Console.WriteLine($"Adding additional files as src: {relativePath}");
                
                await using var fs = File.Open(
                    additionalSrcFile,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite
                );

                var entry = nugetPackage.CreateEntry(relativePath);
                await using var entryFs = entry.Open();

                await fs.CopyToAsync(entryFs);
                await entryFs.FlushAsync();
                
                fs.Close();
                entryFs.Close();
            }
            
            Console.WriteLine("Finishing package and copying to output directory");
            
            nugetPackage.Dispose();
            File.Move(nugetFilePath, Path.Combine(outputLocation, Path.GetFileName(nugetFilePath)), true);
        }
        
        Console.WriteLine("Building and packing shared projects");

        foreach (var sharedProject in sharedProjects)
        {
            await BuildProject(
                sharedProject,
                buildConfiguration
            );

            var nugetFilePath = await PackProject(
                sharedProject,
                TmpDir,
                buildConfiguration
            );
            
            File.Move(nugetFilePath, Path.Combine(outputLocation, Path.GetFileName(nugetFilePath)), true);
        }
    }

    private async Task BuildProject(string file, string configuration)
    {
        var basePath = Path.GetFullPath(Path.GetDirectoryName(file)!);
        var fileName = Path.GetFileName(file);

        await CommandHelper.Run(
            "/usr/bin/dotnet",
            $"build {fileName} --configuration {configuration}",
            basePath
        );
    }

    private async Task<string> PackProject(string file, string output, string configuration)
    {
        var basePath = Path.GetFullPath(Path.GetDirectoryName(file)!);
        var fileName = Path.GetFileName(file);

        await CommandHelper.Run(
            "/usr/bin/dotnet",
            $"pack {fileName} --output {output} --configuration {configuration}",
            basePath
        );

        var nugetFilesPaths = Directory.GetFiles(TmpDir, "*.nupkg", SearchOption.TopDirectoryOnly);

        if (nugetFilesPaths.Length == 0)
            throw new Exception("No nuget packages were built");

        if (nugetFilesPaths.Length > 1)
            throw new Exception("More than one nuget package has been built");

        return nugetFilesPaths.First();
    }

    private async Task CleanDependencies(ZipArchive nugetPackage)
    {
        var nuspecEntry = nugetPackage.Entries.FirstOrDefault(x => x.Name.EndsWith(".nuspec"));

        if (nuspecEntry == null)
        {
            Console.WriteLine("No nuspec file to modify found in nuget package");
            return;
        }

        await ModifyXmlInPackage(nugetPackage, nuspecEntry, document =>
        {
            var ns = document.Root!.GetDefaultNamespace();
            
            var metadata = document.Root!.Element(ns + "metadata")!;
        
            var id = metadata.Element(ns + "id")!.Value;

            // Skip the removal of moonlight references when
            // we are packing moonlight itself
            if (id.StartsWith("Moonlight."))
                return [];

            return document
                .Descendants(ns + "dependency")
                .Where(x => x.Attribute("id")?.Value.StartsWith("Moonlight.") ?? false);
        });
    }

    private async Task<ZipArchiveEntry> ModifyXmlInPackage(
        ZipArchive archive,
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

        var newEntry = archive.CreateEntry(oldPath);
        var newFs = newEntry.Open();

        await document.SaveAsync(newFs, SaveOptions.None, CancellationToken.None);

        await newFs.FlushAsync();
        newFs.Close();

        return newEntry;
    }

    private async Task RemoveContentFiles(ZipArchive nugetPackage)
    {
        // Remove all content files
        foreach (var entry in nugetPackage.Entries.ToArray())
        {
            if (!entry.FullName.StartsWith("contentFiles") && !entry.FullName.StartsWith("content"))
                continue;

            Console.WriteLine($"Removing content file: {entry.FullName}");
            entry.Delete();
        }

        // Remove references to those files in the nuspec file
        var nuspecFile = nugetPackage.Entries.FirstOrDefault(x => x.Name.EndsWith(".nuspec"));

        if (nuspecFile != null)
        {
            Console.WriteLine("Removing references to content files in the nuspec files");

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
    }
}