using System.IO.Compression;
using Moonlight.Core.Database;
using Moonlight.Core.Extensions;
using Moonlight.Core.Interfaces;

namespace Moonlight.Core.Implementations.Diagnose;

public class DatabaseDiagnoseAction : IDiagnoseAction
{
    public async Task GenerateReport(ZipArchive archive, IServiceProvider serviceProvider)
    {
        var dataContext = serviceProvider.GetRequiredService<DataContext>();

        var content = "";

        content += $"Provider: {dataContext.Database.ProviderName}\n";

        await archive.AddText("database.txt", content);
    }
}