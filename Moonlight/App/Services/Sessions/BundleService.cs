using Logging.Net;

namespace Moonlight.App.Services.Sessions;

public class BundleService
{
    public BundleService(ConfigService configService)
    {
        var url = configService
            .GetSection("Moonlight")
            .GetValue<string>("AppUrl");
        
        #region JS

        JsFiles = new();

        JsFiles.AddRange(new[]
        {
            url + "/_framework/blazor.server.js",
            url + "/assets/plugins/global/plugins.bundle.js",
            url + "/_content/XtermBlazor/XtermBlazor.min.js",
            url + "/_content/BlazorTable/BlazorTable.min.js",
            url + "/_content/CurrieTechnologies.Razor.SweetAlert2/sweetAlert2.min.js",
            url + "/_content/Blazor.ContextMenu/blazorContextMenu.min.js",
            "https://www.google.com/recaptcha/api.js",
            "https://cdn.jsdelivr.net/npm/xterm-addon-fit@0.5.0/lib/xterm-addon-fit.min.js",
            "https://cdn.jsdelivr.net/npm/xterm-addon-search@0.8.2/lib/xterm-addon-search.min.js",
            "https://cdn.jsdelivr.net/npm/xterm-addon-web-links@0.5.0/lib/xterm-addon-web-links.min.js",
            url + "/_content/BlazorMonaco/lib/monaco-editor/min/vs/loader.js",
            "require.config({ paths: { 'vs': '/_content/BlazorMonaco/lib/monaco-editor/min/vs' } });",
            url + "/_content/BlazorMonaco/lib/monaco-editor/min/vs/editor/editor.main.js",
            url + "/_content/BlazorMonaco/jsInterop.js",
            url + "/assets/js/scripts.bundle.js",
            url + "/assets/js/moonlight.js",
            "moonlight.loading.registerXterm();",
            url + "/_content/Blazor-ApexCharts/js/apex-charts.min.js",
            url + "/_content/Blazor-ApexCharts/js/blazor-apex-charts.js"
        });

        #endregion

        #region CSS

        CssFiles = new();

        CssFiles.AddRange(new[]
        {
            "https://fonts.googleapis.com/css?family=Poppins:300,400,500,600,700",
            url + "/assets/css/style.bundle.css",
            url + "/assets/css/flashbang.css",
            url + "/assets/css/snow.css",
            url + "/assets/css/utils.css",
            url + "/assets/css/blazor.css",
            url + "/_content/XtermBlazor/XtermBlazor.css",
            url + "/_content/BlazorMonaco/lib/monaco-editor/min/vs/editor/editor.main.css",
            url + "/_content/Blazor.ContextMenu/blazorContextMenu.min.css",
            url + "/assets/plugins/global/plugins.bundle.css"
        });
        
        #endregion

        CacheId = Guid.NewGuid().ToString();

        Task.Run(Bundle);
    }

    // Javascript
    public string BundledJs { get; private set; }
    public readonly List<string> JsFiles;

    // CSS
    public string BundledCss { get; private set; }
    public readonly List<string> CssFiles;

    // States
    public string CacheId { get; private set; }
    public bool BundledFinished { get; set; } = false;
    private bool IsBundling { get; set; } = false;

    private async Task Bundle()
    {
        if (!IsBundling)
            IsBundling = true;

        Logger.Info("Bundling js and css files");

        BundledJs = "";
        BundledCss = "";

        BundledJs = await BundleFiles(
            JsFiles
        );

        BundledCss = await BundleFiles(
            CssFiles
        );

        Logger.Info("Successfully bundled");
        BundledFinished = true;
    }

    private async Task<string> BundleFiles(IEnumerable<string> items)
    {
        var bundled = "";

        using HttpClient client = new HttpClient();
        foreach (string item in items)
        {
            // Item is a url, fetch it
            if (item.StartsWith("http"))
            {
                try
                {
                    var jsCode = await client.GetStringAsync(item);
                    bundled += jsCode + "\n";
                }
                catch (Exception e)
                {
                    Logger.Warn($"Error fetching '{item}' while bundling");
                    Logger.Warn(e);
                }
            }
            else // If not, it is probably a manual addition, so add it
                bundled += item + "\n";
        }

        return bundled;
    }
}