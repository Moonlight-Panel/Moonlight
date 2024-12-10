using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using MoonCore.Helpers;

namespace Moonlight.ApiServer.Helpers;

public class BundleAssetFileProvider : IFileProvider
{
    public IDirectoryContents GetDirectoryContents(string subpath)
     => NotFoundDirectoryContents.Singleton;

    public IFileInfo GetFileInfo(string subpath)
    {
        if(subpath != "/css/bundle.css")
            return new NotFoundFileInfo(subpath);

        var physicalPath = PathBuilder.File("storage", "tmp", "bundle.css");
        
        if(!File.Exists(physicalPath))
            return new NotFoundFileInfo(subpath);

        return new PhysicalFileInfo(new FileInfo(physicalPath));
    }

    public IChangeToken Watch(string filter)
     => NullChangeToken.Singleton;
}