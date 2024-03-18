using System.IO.Compression;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database;
using Moonlight.Core.Extensions;
using Moonlight.Core.Interfaces;
using Newtonsoft.Json;

namespace Moonlight.Core.Implementations.Diagnose;

public class ConfigDiagnoseAction : IDiagnoseAction
{
    public async Task GenerateReport(ZipArchive archive, IServiceProvider serviceProvider)
    {
        var config = serviceProvider.GetRequiredService<ConfigService<CoreConfiguration>>();

        var configJson = JsonConvert.SerializeObject(config.Get());
        var configCopy = JsonConvert.DeserializeObject<CoreConfiguration>(configJson)!;

        configCopy.Database.Password = IsEmpty(configCopy.Database.Password);
        configCopy.Security.Token = IsEmpty(configCopy.Security.Token);

        await archive.AddText("configs/core.json", JsonConvert.SerializeObject(configCopy, Formatting.Indented));
    }
    
    private string IsEmpty(string x) => string.IsNullOrEmpty(x) ? "IS EMPTY" : "IS NOT EMPTY";
}