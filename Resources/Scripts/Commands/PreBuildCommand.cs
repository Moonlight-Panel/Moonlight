using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Cocona;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scripts.Helpers;

namespace Scripts.Commands;

public class PreBuildCommand
{
    private readonly CommandHelper CommandHelper;
    private const string GeneratedStart = "// MLBUILD Generated Start";
    private const string GeneratedEnd = "// MLBUILD Generated End";
    private const string GeneratedHook = "// MLBUILD_PLUGIN_STARTUP_HERE";

    public PreBuildCommand(CommandHelper commandHelper)
    {
        CommandHelper = commandHelper;
    }

    [Command("prebuild")]
    public async Task Prebuild(
        [Argument] string moonlightDir,
        [Argument] string nugetDir
    )
    {
        var dependencies = await GetDependenciesFromNuget(nugetDir);

        Console.WriteLine("Following plugins found:");

        foreach (var dependency in dependencies)
        {
            Console.WriteLine($"{dependency.Id} ({dependency.Version}) [{dependency.Tags}]");
        }

        var csProjFiles = Directory.GetFiles(moonlightDir, "*.csproj", SearchOption.AllDirectories);

        try
        {
            Console.WriteLine("Adjusting csproj files");
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
                    Console.WriteLine($"No package tags found in {Path.GetFullPath(csProjFile)}. Skipping it");
                    continue;
                }

                var packageTags = packageTagsElements.First().Value;

                var dependenciesToAdd = dependencies
                    .Where(x => x.Tags.Contains(packageTags, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();

                await RemoveDependencies(csProjFile);
                await AddDependencies(csProjFile, dependenciesToAdd);
            }

            Console.WriteLine("Restoring projects");
            foreach (var csProjFile in csProjFiles)
            {
                await RestoreProject(csProjFile, nugetDir);
            }

            Console.WriteLine("Generating plugin startup");

            string[] validTags = ["apiserver", "frontend"];

            foreach (var currentTag in validTags)
            {
                Console.WriteLine($"Checking for '{currentTag}' projects");

                foreach (var csProjFile in csProjFiles)
                {
                    var tags = await GetTagsFromCsproj(csProjFile);

                    if (string.IsNullOrEmpty(tags))
                    {
                        Console.WriteLine($"No package tags found in {Path.GetFullPath(csProjFile)}. Skipping it");
                        continue;
                    }

                    if (!tags.Contains(currentTag))
                        continue;

                    var currentDeps = dependencies
                        .Where(x => x.Tags.Contains(currentTag))
                        .ToArray();

                    var classPaths = await FindStartupClasses(currentDeps);

                    var code = new StringBuilder();

                    code.AppendLine(GeneratedStart);

                    foreach (var path in classPaths)
                        code.AppendLine($"pluginStartups.Add(new global::{path}());");

                    code.AppendLine(GeneratedEnd);

                    var filesToSearch = Directory.GetFiles(
                        Path.GetDirectoryName(csProjFile)!,
                        "*.cs",
                        SearchOption.AllDirectories
                    );

                    foreach (var file in filesToSearch)
                    {
                        var content = await File.ReadAllTextAsync(file);

                        if (!content.Contains(GeneratedHook, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        Console.WriteLine($"Injecting generated code to: {Path.GetFullPath(file)}");

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
            Console.WriteLine("An error occured while prebuilding moonlight. Removing csproj modifications");

            foreach (var csProjFile in csProjFiles)
                await RemoveDependencies(csProjFile);

            await RemoveGeneratedCode(moonlightDir);

            throw;
        }
    }

    [Command("prebuild-reset")]
    public async Task PrebuildReset(
        [Argument] string moonlightDir
    )
    {
        var csProjFiles = Directory.GetFiles(moonlightDir, "*.csproj", SearchOption.AllDirectories);

        Console.WriteLine("Reverting csproj changes");

        foreach (var csProjFile in csProjFiles)
            await RemoveDependencies(csProjFile);

        Console.WriteLine("Removing generated code");
        await RemoveGeneratedCode(moonlightDir);
    }

    [Command("test")]
    public async Task Test(
        [Argument] string nugetDir
    )
    {
        var dependencies = await GetDependenciesFromNuget(nugetDir);

        await FindStartupClasses(dependencies);
    }

    private async Task<Dependency[]> GetDependenciesFromNuget(string nugetDir)
    {
        var nugetFiles = Directory.GetFiles(nugetDir, "*.nupkg", SearchOption.AllDirectories);
        var dependencies = new List<Dependency>();

        foreach (var nugetFile in nugetFiles)
        {
            var dependency = await GetDependencyFromPackage(nugetFile);
            dependencies.Add(dependency);
        }

        return dependencies.ToArray();
    }

    private async Task<Dependency> GetDependencyFromPackage(string path)
    {
        using var nugetPackage = ZipFile.Open(path, ZipArchiveMode.Read);

        var nuspecEntry = nugetPackage.Entries.First(x => x.Name.EndsWith(".nuspec"));
        await using var nuspecFs = nuspecEntry.Open();

        var nuspec = await XDocument.LoadAsync(nuspecFs, LoadOptions.None, CancellationToken.None);

        var ns = nuspec.Root!.GetDefaultNamespace();
        var metadata = nuspec.Root!.Element(ns + "metadata")!;

        var id = metadata.Element(ns + "id")!.Value;
        var version = metadata.Element(ns + "version")!.Value;
        var tags = metadata.Element(ns + "tags")!.Value;

        nuspecFs.Close();

        return new Dependency()
        {
            Id = id,
            Version = version,
            Tags = tags
        };
    }

    private async Task AddDependencies(string path, Dependency[] dependencies)
    {
        await using var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        fs.Position = 0;

        var csProj = await XDocument.LoadAsync(fs, LoadOptions.None, CancellationToken.None);

        var project = csProj.Element("Project")!;

        var itemGroup = new XElement("ItemGroup");
        itemGroup.SetAttributeValue("Label", "MoonlightBuildDeps");

        foreach (var dependency in dependencies)
        {
            var depElement = new XElement("PackageReference");
            depElement.SetAttributeValue("Include", dependency.Id);
            depElement.SetAttributeValue("Version", dependency.Version);

            itemGroup.Add(depElement);
        }

        project.Add(itemGroup);

        fs.Position = 0;
        await csProj.SaveAsync(fs, SaveOptions.None, CancellationToken.None);
    }

    private Task RemoveDependencies(string path)
    {
        var csProj = XDocument.Load(path, LoadOptions.None);

        var itemGroupsToRemove = csProj
            .Descendants("ItemGroup")
            .Where(x => x.Attribute("Label")?.Value.Contains("MoonlightBuildDeps") ?? false)
            .ToArray();

        itemGroupsToRemove.Remove();

        csProj.Save(path, SaveOptions.None);

        return Task.CompletedTask;
    }

    private async Task RestoreProject(string file, string nugetPath)
    {
        var basePath = Path.GetFullPath(Path.GetDirectoryName(file)!);
        var fileName = Path.GetFileName(file);
        var nugetPathFull = Path.GetFullPath(nugetPath);

        Console.WriteLine($"Restore: {basePath} - {fileName}");

        await CommandHelper.Run(
            "/usr/bin/dotnet",
            $"restore {fileName} --source {nugetPathFull}",
            basePath
        );
    }

    private async Task<string[]> FindStartupClasses(Dependency[] dependencies)
    {
        var result = new List<string>();

        var nugetPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages"
        );

        var filesToScan = dependencies.SelectMany(dependency =>
            {
                var dependencySrcPath = Path.Combine(nugetPath, dependency.Id.ToLower(), dependency.Version, "src");

                Console.WriteLine($"Checking {dependencySrcPath}");

                if (!Directory.Exists(dependencySrcPath))
                    return [];

                return Directory.GetFiles(dependencySrcPath, "*.cs", SearchOption.AllDirectories);
            }
        ).ToArray();

        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        var trees = new List<SyntaxTree>();

        foreach (var file in filesToScan)
        {
            Console.WriteLine($"Reading {file}");

            var content = await File.ReadAllTextAsync(file);
            var tree = CSharpSyntaxTree.ParseText(content);
            trees.Add(tree);
        }

        var compilation = CSharpCompilation.Create("Analysis", trees, [mscorlib]);

        foreach (var tree in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            var root = await tree.GetRootAsync();

            var classDeclarations = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                var symbol = model.GetDeclaredSymbol(classDeclaration);

                if (symbol == null)
                    continue;

                var hasAttribute = symbol.GetAttributes().Any(attr =>
                {
                    if (attr.AttributeClass == null)
                        return false;

                    return attr.AttributeClass.Name == "PluginStartup";
                });

                if (!hasAttribute)
                    continue;

                var classPath =
                    $"{symbol.ContainingNamespace.ToDisplayString()}.{classDeclaration.Identifier.ValueText}";

                Console.WriteLine($"Detected startup in class: {classPath}");
                result.Add(classPath);
            }
        }

        return result.ToArray();
    }

    private async Task<string> GetTagsFromCsproj(string csProjFile)
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
            return "";

        return packageTagsElements.First().Value;
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
            // We dont want to replace ourself
            if (file.Contains("PreBuildCommand.cs"))
                continue;

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

    private record Dependency
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Tags { get; set; }
    }
}