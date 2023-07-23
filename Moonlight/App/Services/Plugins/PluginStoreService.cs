using System.Text;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Octokit;

namespace Moonlight.App.Services.Plugins;

public class PluginStoreService
{
    private readonly GitHubClient Client;
    private readonly PluginService PluginService;

    public PluginStoreService(PluginService pluginService)
    {
        PluginService = pluginService;
        Client = new(new ProductHeaderValue("Moonlight-Panel"));
    }

    public async Task<OfficialMoonlightPlugin[]> GetPlugins()
    {
        var items = await Client.Repository.Content.GetAllContents("Moonlight-Panel", "OfficialPlugins");

        if (items == null)
        {
            Logger.Fatal("Unable to read plugin repo contents");
            return Array.Empty<OfficialMoonlightPlugin>();
        }

        return items
            .Where(x => x.Type == ContentType.Dir)
            .Select(x => new OfficialMoonlightPlugin()
            {
                Name = x.Name
            })
            .ToArray();
    }

    public async Task<string> GetPluginReadme(OfficialMoonlightPlugin plugin)
    {
        var rawReadme = await Client.Repository.Content
            .GetRawContent("Moonlight-Panel", "OfficialPlugins", $"{plugin.Name}/README.md");

        if (rawReadme == null)
            return "Error";

        return Encoding.UTF8.GetString(rawReadme);
    }

    public async Task InstallPlugin(OfficialMoonlightPlugin plugin, bool updating = false)
    {
        var rawPlugin = await Client.Repository.Content
            .GetRawContent("Moonlight-Panel", "OfficialPlugins", $"{plugin.Name}/{plugin.Name}.dll");

        if (updating)
        {
            await File.WriteAllBytesAsync(PathBuilder.File("storage", "plugins", $"{plugin.Name}.dll.cache"), rawPlugin);
            return;
        }
        
        await File.WriteAllBytesAsync(PathBuilder.File("storage", "plugins", $"{plugin.Name}.dll"), rawPlugin);
        await PluginService.ReloadPlugins();
    }
}