using System.IO.Compression;
using Moonlight.Core.Extensions;
using Moonlight.Core.Interfaces;
using Moonlight.Core.Services;

namespace Moonlight.Core.Implementations.Diagnose;

public class FeatureDiagnoseAction : IDiagnoseAction
{
    public async Task GenerateReport(ZipArchive archive, IServiceProvider serviceProvider)
    {
        var featureService = serviceProvider.GetRequiredService<FeatureService>();

        var content = "Loaded features:\n\n";

        foreach (var feature in await featureService.GetLoadedFeatures())
        {
            content += $"Name: {feature.Name}\n";
            content += $"Author: {feature.Author}\n";
            content += $"Issue Tracker: {feature.IssueTracker}\n";
            content += $"Assembly name: {feature.GetType().FullName}\n";
            content += "\n";
        }
        
        await archive.AddText("features.txt", content);
    }
}