using System.IO.Compression;
using MoonCore.Helpers;
using Moonlight.Core.Extensions;
using Moonlight.Core.Interfaces;

namespace Moonlight.Core.Implementations.Diagnose;

public class LogDiagnoseAction : IDiagnoseAction
{
    public async Task GenerateReport(ZipArchive archive, IServiceProvider serviceProvider)
    {
        var path = PathBuilder.File("storage", "logs", "moonlight.log");
        
        if(!File.Exists(path))
            return;
        
        // Add current log
        // We need to open the file this way because we need to specify the access and share mode directly
        // in order to read from a file which is currently written to
        var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var sr = new StreamReader(fs);
        var log = await sr.ReadToEndAsync();
        sr.Close();
        fs.Close();
        
        await archive.AddText("log.txt", log);
    }
}