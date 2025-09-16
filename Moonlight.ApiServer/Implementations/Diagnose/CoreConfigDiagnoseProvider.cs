using System.Diagnostics;
using System.IO.Compression;
using MoonCore.Yaml;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Extensions;
using Moonlight.ApiServer.Interfaces;

namespace Moonlight.ApiServer.Implementations.Diagnose;

public class CoreConfigDiagnoseProvider : IDiagnoseProvider
{
    private readonly AppConfiguration Configuration;

    public CoreConfigDiagnoseProvider(AppConfiguration configuration)
    {
        Configuration = configuration;
    }

    private string CheckForNullOrEmpty(string? content)
    {
        return string.IsNullOrEmpty(content)
            ? "ISEMPTY"
            : "ISNOTEMPTY";
    }

    public async Task ModifyZipArchiveAsync(ZipArchive archive)
    {
        try
        {
            var configString = YamlSerializer.Serialize(Configuration);
            var configuration = YamlSerializer.Deserialize<AppConfiguration>(configString);
            
            configuration.Database.Password = CheckForNullOrEmpty(configuration.Database.Password);
            configuration.Authentication.Secret = CheckForNullOrEmpty(configuration.Authentication.Secret);
            configuration.SignalR.RedisConnectionString = CheckForNullOrEmpty(configuration.SignalR.RedisConnectionString);

            await archive.AddTextAsync(
                "core/config.txt",
                YamlSerializer.Serialize(configuration)
            );
        }
        catch (Exception e)
        {
            await archive.AddTextAsync("core/config.txt", $"Unable to load config: {e.ToStringDemystified()}");
        }
    }
}