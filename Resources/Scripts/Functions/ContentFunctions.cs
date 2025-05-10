using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Scripts.Functions;

public static class ContentFunctions
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
        
        Console.WriteLine(string.Join(", ",  args
            .Skip(1)
            .Select(x => new Regex(x))));

        if (!File.Exists(nugetPath))
        {
            Console.WriteLine("The provided file does not exist");
            return;
        }

        Console.WriteLine("Modding nuget package...");
        using var zipFile = ZipFile.Open(nugetPath, ZipArchiveMode.Update);

        foreach (var zipArchiveEntry in zipFile.Entries)
        {
            Console.WriteLine(zipArchiveEntry.FullName);
        }

        Console.WriteLine("Searching for files to remove");
        var files = zipFile.Entries
            .Where(x => x.FullName.Trim('/').StartsWith("content"))
            .Where(x =>
            {
                var name = x.FullName
                    .Replace("contentFiles/", "")
                    .Replace("content/", "");
                
                Console.WriteLine(name);
                
                return regexs.Any(y => y.IsMatch(name));
            })
            .ToArray();

        Console.WriteLine($"Found {files.Length} file(s) to remove");
        foreach (var file in files)
            file.Delete();
    }
}