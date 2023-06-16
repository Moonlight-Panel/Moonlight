using Moonlight.App.ApiClients.Modrinth;
using Moonlight.App.ApiClients.Modrinth.Resources;
using Moonlight.App.Exceptions;
using FileAccess = Moonlight.App.Helpers.Files.FileAccess;
using Version = Moonlight.App.ApiClients.Modrinth.Resources.Version;

namespace Moonlight.App.Services.Addon;

public class ServerAddonPluginService
{
    private readonly ModrinthApiHelper ModrinthApiHelper;
    private readonly ServerService ServerService;

    public ServerAddonPluginService(ModrinthApiHelper modrinthApiHelper)
    {
        ModrinthApiHelper = modrinthApiHelper;
    }

    public async Task<Project[]> GetPluginsForVersion(string version, string search = "")
    {
        string resource;
        var filter =
            "[[\"categories:\'bukkit\'\",\"categories:\'paper\'\",\"categories:\'spigot\'\"],[\"versions:" + version + "\"],[\"project_type:mod\"]]";

        if (string.IsNullOrEmpty(search))
            resource = "search?limit=21&index=relevance&facets=" + filter;
        else
            resource = $"search?query={search}&limit=21&index=relevance&facets=" + filter;
        
        var result = await ModrinthApiHelper.Get<Pagination>(resource);

        return result.Hits;
    }

    public async Task InstallPlugin(FileAccess fileAccess, string version, Project project, Action<string>? onStateUpdated = null)
    {
        // Resolve plugin download
        
        onStateUpdated?.Invoke($"Resolving {project.Slug}");
        
        var filter = "game_versions=[\"" + version + "\"]&loaders=[\"bukkit\", \"paper\", \"spigot\"]";
        
        var versions = await ModrinthApiHelper.Get<Version[]>(
            $"project/{project.Slug}/version?" + filter);

        if (!versions.Any())
            throw new DisplayException("No plugin download for your minecraft version found");

        var installVersion = versions.OrderByDescending(x => x.DatePublished).First();
        var fileToInstall = installVersion.Files.First();
        
        // Download plugin in a stream cached mode

        var httpClient = new HttpClient();
        var stream = await httpClient.GetStreamAsync(fileToInstall.Url);
        var dataStream = new MemoryStream(1024 * 1024 * 40);
        await stream.CopyToAsync(dataStream);
        stream.Close();
        dataStream.Position = 0;
        
        // Install plugin

        await fileAccess.SetDir("/");
        
        try
        {
            await fileAccess.MkDir("plugins");
        }
        catch (Exception)
        {
            // Ignored
        }

        await fileAccess.SetDir("plugins");
        
        onStateUpdated?.Invoke($"Installing {project.Slug}");
        await fileAccess.Upload(fileToInstall.Filename, dataStream);
        
        await dataStream.DisposeAsync();
        
        //TODO: At some point of time, create a dependency resolver
    }
}