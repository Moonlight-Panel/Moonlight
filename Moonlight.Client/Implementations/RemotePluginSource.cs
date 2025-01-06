using System.Runtime.Loader;
using MoonCore.Plugins;
using Moonlight.Shared.Misc;

namespace Moonlight.Client.Implementations;

public class RemotePluginSource : IPluginSource
{
    private readonly FrontendConfiguration Configuration;
    private readonly ILogger<RemotePluginSource> Logger;
    private readonly HttpClient HttpClient;

    public RemotePluginSource(
        FrontendConfiguration configuration,
        ILogger<RemotePluginSource> logger,
        HttpClient httpClient
    )
    {
        Configuration = configuration;
        Logger = logger;
        HttpClient = httpClient;
    }

    public async Task Load(AssemblyLoadContext loadContext, List<string> entrypoints)
    {
        entrypoints.AddRange(Configuration.Plugins.Entrypoints);

        foreach (var assembly in Configuration.Plugins.Assemblies)
        {
            try
            {
                var fileStream = await HttpClient.GetStreamAsync($"plugins/{assembly}");
                loadContext.LoadFromStream(fileStream);
            }
            catch (Exception e)
            {
                Logger.LogCritical("Unable to load plugin assembly '{assembly}': {e}", assembly, e);
            }
        }
    }
}