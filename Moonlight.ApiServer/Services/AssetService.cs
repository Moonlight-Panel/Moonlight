using MoonCore.Attributes;

namespace Moonlight.ApiServer.Services;

[Singleton]
public class AssetService
{
    public string[] JavascriptFiles { get; private set; }

    private bool HasBeenCollected = false;
    
    private readonly List<string> AdditionalCssAssets = new();
    private readonly List<string> AdditionalJavascriptAssets = new();
    
    private readonly PluginService PluginService;

    public AssetService(PluginService pluginService)
    {
        PluginService = pluginService;
    }

    public void CollectAssets()
    {
        // Javascript
        var jsFiles = new List<string>();
        
        jsFiles.AddRange(AdditionalJavascriptAssets);
        jsFiles.AddRange(PluginService.AssetMap.Keys.Where(x => x.EndsWith(".js")));

        JavascriptFiles = jsFiles.ToArray();
    }
    
    public void AddJavascriptAsset(string asset)
        => AdditionalJavascriptAssets.Add(asset);

    public string[] GetJavascriptAssets()
    {
        if (HasBeenCollected)
            return JavascriptFiles;
        
        CollectAssets();

        return JavascriptFiles;
    }
}