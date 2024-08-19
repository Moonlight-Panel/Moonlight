using System.IO.Compression;
using Moonlight.ApiServer.App.Extensions;
using Moonlight.ApiServer.App.Interfaces;
using Moonlight.ApiServer.App.PluginApi;

namespace Moonlight.ApiServer.App.Implementations.Diagnose;

public class PluginDiagnoseReporter : IDiagnoseReporter
{
    public async Task GenerateReport(ZipArchive archive, IServiceProvider provider)
    {
        var pluginService = provider.GetRequiredService<PluginService>();

        var text = "";

        // Plugin assemblies
        text += "Plugin assemblies:\n";

        foreach (var assembly in pluginService.PluginAssemblies)
            text += $"{assembly.FullName ?? "N/A"} ({assembly.Location})\n";
        
        // Library assemblies
        text += "\nLibrary assemblies:\n";

        foreach (var assembly in pluginService.LibraryAssemblies)
            text += $"{assembly.FullName ?? "N/A"} ({assembly.Location})\n";
        
        // Plugins
        text += "\nPlugins:\n";

        foreach (var plugin in pluginService.LoadedPlugins)
            text += $"{plugin.GetType().FullName ?? "N/A"}\n";
        
        // Finalize
        await archive.AddText("plugins.txt", text);
    }
}