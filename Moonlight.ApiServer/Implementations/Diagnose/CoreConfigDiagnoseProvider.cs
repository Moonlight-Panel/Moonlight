using System.IO.Compression;
using System.Text;
using System.Text.Json;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Extensions;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models.Diagnose;

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
        var config =  JsonSerializer.Deserialize<AppConfiguration>(json);

        if (config == null)
        {
            await archive.AddText("core/config.txt","Could not fetch config.");
            
            return;
        }
 
        config.Database.Password = CheckForNullOrEmpty(config.Database.Password);
                        
        config.Authentication.OAuth2.ClientSecret = CheckForNullOrEmpty(config.Authentication.OAuth2.ClientSecret);
                        
        config.Authentication.OAuth2.Secret = CheckForNullOrEmpty(config.Authentication.OAuth2.Secret);

        config.Authentication.Secret = CheckForNullOrEmpty(config.Authentication.Secret);

        await archive.AddText("core/config.txt",
            JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true }));
    }
}