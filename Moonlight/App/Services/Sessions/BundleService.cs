using Logging.Net;

namespace Moonlight.App.Services.Sessions;

public class BundleService
{
    private readonly ConfigService ConfigService;

    public BundleService(ConfigService configService)
    {
        ConfigService = configService;

        CacheId = Guid.NewGuid().ToString();

        Task.Run(Bundle);
    }
    
    public string BundledJs { get; private set; }
    public string BundledCss { get; private set; }
    public string CacheId { get; private set; }
    public bool BundledFinished { get; set; } = false;
    private bool IsBundling { get; set; } = false;

    public async Task Bundle()
    {
        if (!IsBundling)
            IsBundling = true;
        
        Logger.Info("Bundling js and css files");
        
        BundledJs = "";
        BundledCss = "";

        var url = ConfigService
            .GetSection("Moonlight")
            .GetValue<string>("AppUrl");
        
        BundledJs = await BundleFiles(
            new[]
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
            }
        );

        BundledCss = await BundleFiles(
            new[]
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
                url + "/assets/plugins/global/plugins.bundle.css",
            }
        );
        
        Logger.Info("Successfully bundled");
        BundledFinished = true;
    }
    
    private async Task<string> BundleFiles(string[] jsUrls)
    {
        var bundledJs = "";

        using HttpClient client = new HttpClient();
        foreach (string url in jsUrls)
        {
            if (url.StartsWith("http"))
            {
                var jsCode = await client.GetStringAsync(url);
                bundledJs += jsCode + "\n";
            }
            else
                bundledJs += url + "\n";
        }

        return bundledJs;
    }
}