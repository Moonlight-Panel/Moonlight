using System.Text;
using Moonlight.App.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Moonlight.App.Services.Minecraft;

public class FabricService
{
    private readonly HttpClient Client;

    public FabricService()
    {
        Client = new();
    }

    public async Task<string> GetLatestInstallerVersion()
    {
        var data = await Client
            .GetStringAsync("https://meta.fabricmc.net/v2/versions/installer");

        var x = JsonConvert.DeserializeObject<JObject[]>(data) ?? Array.Empty<JObject>();

        var stableVersions = new List<string>();

        foreach (var y in x)
        {
            var section = new ConfigurationBuilder().AddJsonStream(
                new MemoryStream(Encoding.ASCII.GetBytes(
                        y.Root.ToString()
                    )
                )
            ).Build();
            
            if (section.GetValue<bool>("stable"))
            {
                stableVersions.Add(section.GetValue<string>("version"));
            }
        }

        return ParseHelper.GetHighestVersion(stableVersions.ToArray());
    }
    
    public async Task<string> GetLatestLoaderVersion()
    {
        var data = await Client
            .GetStringAsync("https://meta.fabricmc.net/v2/versions/loader");

        var x = JsonConvert.DeserializeObject<JObject[]>(data) ?? Array.Empty<JObject>();

        var stableVersions = new List<string>();

        foreach (var y in x)
        {
            var section = new ConfigurationBuilder().AddJsonStream(
                new MemoryStream(Encoding.ASCII.GetBytes(
                        y.Root.ToString()
                    )
                )
            ).Build();
            
            if (section.GetValue<bool>("stable"))
            {
                stableVersions.Add(section.GetValue<string>("version"));
            }
        }

        return ParseHelper.GetHighestVersion(stableVersions.ToArray());
    }
    public async Task<string[]> GetGameVersions()
    {
        var data = await Client
            .GetStringAsync("https://meta.fabricmc.net/v2/versions/game");

        var x = JsonConvert.DeserializeObject<JObject[]>(data) ?? Array.Empty<JObject>();

        var stableVersions = new List<string>();

        foreach (var y in x)
        {
            var section = new ConfigurationBuilder().AddJsonStream(
                new MemoryStream(Encoding.ASCII.GetBytes(
                        y.Root.ToString()
                    )
                )
            ).Build();
            
            if (section.GetValue<bool>("stable"))
            {
                stableVersions.Add(section.GetValue<string>("version"));
            }
        }

        return stableVersions.ToArray();
    }
}