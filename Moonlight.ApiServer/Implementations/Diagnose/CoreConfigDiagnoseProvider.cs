using System.IO.Compression;
using System.Text.Json;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Extensions;
using Moonlight.ApiServer.Interfaces;

namespace Moonlight.ApiServer.Implementations.Diagnose;

public class CoreConfigDiagnoseProvider : IDiagnoseProvider
{
    private readonly AppConfiguration Config;

    public CoreConfigDiagnoseProvider(AppConfiguration config)
    {
        Config = config;
    }

    private string CheckForNullOrEmpty(string? content)
    {
        return string.IsNullOrEmpty(content)
            ? "ISEMPTY"
            : "ISNOTEMPTY";
    }

    public async Task ModifyZipArchive(ZipArchive archive)
    {
        var json = JsonSerializer.Serialize(Config);
        var config = JsonSerializer.Deserialize<AppConfiguration>(json);

        if (config == null)
        {
            await archive.AddText("core/config.txt", "Could not fetch config.");
            return;
        }

        config.Database.Password = CheckForNullOrEmpty(config.Database.Password);

        config.Authentication.OAuth2.ClientSecret = CheckForNullOrEmpty(config.Authentication.OAuth2.ClientSecret);

        config.Authentication.OAuth2.Secret = CheckForNullOrEmpty(config.Authentication.OAuth2.Secret);

        config.Authentication.Secret = CheckForNullOrEmpty(config.Authentication.Secret);

        config.Authentication.OAuth2.ClientId = CheckForNullOrEmpty(config.Authentication.OAuth2.ClientId);

        await archive.AddText(
            "core/config.txt",
            JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions()
                {
                    WriteIndented = true
                }
            )
        );
    }
}