using System.IO.Compression;

namespace Scripts.Functions;

public static class SrcFunctions
{
    public static async Task Run(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Please provide the path to a nuget file, a search pattern and a path");
            return;
        }

        var nugetPath = args[0];
        var path = args[1];
        var pattern = args[2];

        if (!File.Exists(nugetPath))
        {
            Console.WriteLine("The provided file does not exist");
            return;
        }

        Console.WriteLine("Modding nuget package...");
        using var zipFile = ZipFile.Open(nugetPath, ZipArchiveMode.Update);

        var filesToAdd = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);

        foreach (var file in filesToAdd)
        {
            var name = file.Replace(path, "").Replace("\\", "/");
         
            Console.WriteLine($"{file} => /src/{name}");
            
            var entry = zipFile.CreateEntry($"src/{name}");
            await using var entryStream = entry.Open();

            await using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            await fs.CopyToAsync(entryStream);
            fs.Close();

            await entryStream.FlushAsync();
            entryStream.Close();
        }
    }
}