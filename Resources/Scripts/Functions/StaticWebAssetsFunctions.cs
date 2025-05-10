using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Scripts.Functions;

public static class StaticWebAssetsFunctions
{
    public static async Task Run(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Please provide the path to a nuget file and at least one regex expression");
            return;
        }

        var nugetPath = args[0];

        var regexs = args
            .Skip(1)
            .Select(x => new Regex(x))
            .ToArray();

        if (!File.Exists(nugetPath))
        {
            Console.WriteLine("The provided file does not exist");
            return;
        }

        Console.WriteLine("Modding nuget package...");
        using var zipFile = ZipFile.Open(nugetPath, ZipArchiveMode.Update);

        Console.WriteLine("Searching for files to remove");
        var files = zipFile.Entries
            .Where(x => x.FullName.Trim('/').StartsWith("staticwebassets"))
            .Where(x =>
            {
                var name = x.FullName.Replace("staticwebassets/", "");
                
                return regexs.Any(y => y.IsMatch(name));
            })
            .ToArray();

        Console.WriteLine($"Found {files.Length} file(s) to remove");
        foreach (var file in files)
            file.Delete();

        Console.WriteLine("Modifying static web assets build target");
        var oldBuildTargetEntry = zipFile
            .Entries
            .FirstOrDefault(x => x.FullName == "build/Microsoft.AspNetCore.StaticWebAssets.props");

        if (oldBuildTargetEntry == null)
        {
            Console.WriteLine("Build target file not found in nuget packages");
            return;
        }

        await using var oldBuildTargetStream = oldBuildTargetEntry.Open();

        var contentXml = await XDocument.LoadAsync(
            oldBuildTargetStream,
            LoadOptions.None,
            CancellationToken.None
        );

        oldBuildTargetStream.Close();
        oldBuildTargetEntry.Delete();

        var assetRefsToRemove = contentXml
            .Descendants("StaticWebAsset")
            .Where(asset =>
            {
                var element = asset.Element("RelativePath");

                if (element == null)
                    return false;

                return regexs.Any(y => y.IsMatch(element.Value));
            })
            .ToArray();

        foreach (var asset in assetRefsToRemove)
            asset.Remove();
        
        var newBuildTargetEntry = zipFile.CreateEntry("build/Microsoft.AspNetCore.StaticWebAssets.props");
        await using var newBuildTargetStream = newBuildTargetEntry.Open();

        await contentXml.SaveAsync(newBuildTargetStream, SaveOptions.None, CancellationToken.None);
        
        newBuildTargetStream.Close();
    }
}