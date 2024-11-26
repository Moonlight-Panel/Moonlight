using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer.Helpers;

public class PluginAssetFileProvider : IFileProvider
{
    private readonly PluginService PluginService;

    public PluginAssetFileProvider(PluginService pluginService)
    {
        PluginService = pluginService;
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        if (!PluginService.AssetMap.TryGetValue(subpath, out var physicalPath))
            return new NotFoundFileInfo(subpath);
        
        if (!File.Exists(physicalPath))
            return new NotFoundFileInfo(subpath);

        return new PhysicalFileInfo(new FileInfo(physicalPath));
    }

    public IChangeToken Watch(string filter)
    {
        return NullChangeToken.Singleton;
    }
}