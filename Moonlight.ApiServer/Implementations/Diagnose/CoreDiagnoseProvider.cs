using System.Text;
using System.Text.Json;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models.Diagnose;

namespace Moonlight.ApiServer.Implementations.Diagnose;

public class CoreDiagnoseProvider : IDiagnoseProvider
{
    private readonly AppConfiguration Config;

    public CoreDiagnoseProvider(AppConfiguration config)
    {
        Config = config;
    }

    public DiagnoseEntry[] GetFiles()
    {
        return
        [
            new DiagnoseDirectory()
            {
                Name = "core",
                Children = [
                
                    new DiagnoseFile()
                    {
                        Name = "logs.txt",
                        GetContent = () =>
                        {
                            var logs = File.ReadAllText(PathBuilder.File("storage", "logs", "latest.log"));

                            if (string.IsNullOrEmpty(logs))
                                return Encoding.UTF8.GetBytes("Could not get the latest logs.");
                            
                            return Encoding.UTF8.GetBytes(logs);
                        }
                    },
                    
                    new DiagnoseFile()
                    {
                        Name = "config.txt",
                        GetContent = () =>
                        {
                            var json = JsonSerializer.Serialize(Config);
                            var config =  JsonSerializer.Deserialize<AppConfiguration>(json);

                            if (config == null)
                            {
                                return Encoding.UTF8.GetBytes("Could not fetch config.");
                            }
                            
                            config.Database.Password = CheckForNullOrEmpty(config.Database.Password);
                            
                            config.Authentication.OAuth2.ClientSecret = CheckForNullOrEmpty(config.Authentication.OAuth2.ClientSecret);
                            
                            config.Authentication.OAuth2.Secret = CheckForNullOrEmpty(config.Authentication.OAuth2.Secret);

                            config.Authentication.Secret = CheckForNullOrEmpty(config.Authentication.Secret);

                            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true}));
                        }
                    }
                    
                ]
            }
        ];
    }

    private string CheckForNullOrEmpty(string? content)
    {
        return string.IsNullOrEmpty(content)
            ? "ISEMPTY"
            : "ISNOTEMPTY";
    }
}