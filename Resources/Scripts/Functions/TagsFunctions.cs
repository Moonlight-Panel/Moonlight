using System.Xml.Linq;

namespace Scripts.Functions;

public class TagsFunctions
{
    public static async Task Run(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("You need to specify a directory, tag and at least one command");
            return;
        }

        var rootDir = args[0];
        var tag = args[1];
        var commands = args.Skip(2).ToArray();

        var csprojFiles = Directory.GetFiles(
            rootDir,
            "*.csproj",
            SearchOption.AllDirectories
        );

        foreach (var csprojFile in csprojFiles)
        {
            try
            {
                // Load the .csproj file
                var csprojXml = XElement.Load(csprojFile);

                // Check if <PackageTags> exists within the .csproj file
                var packageTagsElement = csprojXml.Descendants("PackageTags").FirstOrDefault();
                if (packageTagsElement != null)
                {
                    if(!packageTagsElement.Value.Contains(tag))
                        continue;
                    
                    var projectName = Path.GetFileName(Path.GetDirectoryName(csprojFile))!;
                    var projectFile = Path.GetFileName(csprojFile);
                    
                    foreach (var command in commands)
                    {
                        // Replace PROJECT_NAME and PROJECT_FILE with the actual values
                        var bashCommand = command.Replace("PROJECT_NAME", projectName).Replace("PROJECT_FILE", projectFile);
                        RunBashCommand(bashCommand);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {csprojFile}: {ex.Message}");
            }
        }
    }
    
    static void RunBashCommand(string command)
    {
        try
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{command}\"",
            };

            var process = System.Diagnostics.Process.Start(processStartInfo);
            process!.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running bash command: {ex.Message}");
        }
    }
}