using System.IO.Compression;
using Moonlight.Core.Extensions;
using Moonlight.Core.Interfaces;
using Moonlight.Core.Services;

namespace Moonlight.Core.Implementations.Diagnose;

public class PluginsDiagnoseAction : IDiagnoseAction
{
    public async Task GenerateReport(ZipArchive archive, IServiceProvider serviceProvider)
    {
        var pluginService = serviceProvider.GetRequiredService<PluginService>();

        var content = "Loaded plugins:\n\n";

        foreach (var plugin in await pluginService.GetLoadedPlugins())
        {
            content += $"Name: {plugin.Name}\n";
            content += $"Author: {plugin.Author}\n";
            content += $"Issue Tracker: {plugin.IssueTracker}\n";
            content += $"Assembly name: {plugin.GetType().FullName}\n";
            content += "\n";
        }

        await archive.AddText("plugins.txt", content);
    }
}