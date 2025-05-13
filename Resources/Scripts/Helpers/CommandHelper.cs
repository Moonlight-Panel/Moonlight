using System.Diagnostics;

namespace Scripts.Helpers;

public class CommandHelper
{
    public async Task Run(string program, string arguments, string? workingDir = null)
    {
        var process = await RunRaw(program, arguments, workingDir);

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception($"The command '{program} {arguments}' failed with exit code: {process.ExitCode}");
    }
    
    private Task<Process> RunRaw(string program, string arguments, string? workingDir = null)
    {
        var psi = new ProcessStartInfo()
        {
            FileName = program,
            Arguments = arguments,
            WorkingDirectory = string.IsNullOrEmpty(workingDir) ? Directory.GetCurrentDirectory() : workingDir
        };
        
        var process = Process.Start(psi)!;

        return Task.FromResult(process);
    }
}