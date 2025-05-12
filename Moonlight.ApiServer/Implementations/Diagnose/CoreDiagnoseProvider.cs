using System.Text;
using System.Text.Json;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models.Diagnose;

namespace Moonlight.ApiServer.Implementations.Diagnose;

public class CoreDiagnoseProvider : IDiagnoseProvider
{
    public DiagnoseEntry[] GetFiles()
    {
        // TODO:
        // - read logs out from file for the diagnose below
        
        return
        [
            new DiagnoseDirectory()
            {
                Name = "core",
                Children = [
                
                    new DiagnoseFile()
                    {
                        Name = "logs.txt",
                        GetContent = () => Encoding.UTF8.GetBytes("placeholder")
                    },
                    
                    new DiagnoseFile()
                    {
                        Name = "logs.txt",
                        GetContent = () =>
                        {

                            var configJson = File.ReadAllText(PathBuilder.File("storage", "app.json"));

                            var config = JsonSerializer.Deserialize<AppConfiguration>(configJson);

                            if (config == null)
                            {
                                return Encoding.UTF8.GetBytes("could not fetch config");
                            }

                            config.Database.Password = CheckForNullOrEmpty(config.Database.Password);
                            
                            config.Authentication.OAuth2.ClientSecret = CheckForNullOrEmpty(config.Authentication.OAuth2.ClientSecret);
                            
                            config.Authentication.OAuth2.Secret = CheckForNullOrEmpty(config.Authentication.OAuth2.Secret);

                            config.Authentication.Secret = CheckForNullOrEmpty(config.Authentication.Secret);

                            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(config));
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