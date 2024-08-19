using System.IO.Compression;
using System.Text.Json;
using MoonCore.Services;
using Moonlight.ApiServer.App.Configuration;
using Moonlight.ApiServer.App.Extensions;
using Moonlight.ApiServer.App.Interfaces;

namespace Moonlight.ApiServer.App.Implementations.Diagnose;

public class ConfigurationDiagnoseReporter : IDiagnoseReporter
{
    public async Task GenerateReport(ZipArchive archive, IServiceProvider provider)
    {
        var configService = provider.GetRequiredService<ConfigService<AppConfiguration>>();
        
        var configurationClone =
            JsonSerializer.Deserialize<AppConfiguration>(JsonSerializer.Serialize(configService.Get()))!;

        configurationClone.Database.Password = string.IsNullOrEmpty(configurationClone.Database.Password) ? "EMPTY" : "NOT EMPTY";
        configurationClone.Security.Token = string.IsNullOrEmpty(configurationClone.Security.Token) ? "EMPTY" : "NOT EMPTY";

        var json = JsonSerializer.Serialize(configurationClone, new JsonSerializerOptions()
        {
            WriteIndented = true
        });

        await archive.AddText("config.json", json);
    }
}