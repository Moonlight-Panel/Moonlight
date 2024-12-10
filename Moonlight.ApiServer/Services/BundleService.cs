using ExCSS;
using Microsoft.Extensions.Hosting.Internal;
using MoonCore.Helpers;

namespace Moonlight.ApiServer.Services;

public class BundleService : IHostedService
{
    private readonly ILogger<BundleService> Logger;
    private readonly List<string> CssFiles = new();

    private readonly AssetService AssetService;
    private readonly IWebHostEnvironment HostEnvironment;

    public BundleService(ILogger<BundleService> logger, PluginService pluginService, IWebHostEnvironment hostEnvironment, AssetService assetService)
    {
        Logger = logger;
        HostEnvironment = hostEnvironment;
        AssetService = assetService;
    }

    public async Task Bundle(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Collecting css files...");

        var fi = HostEnvironment.WebRootFileProvider.GetFileInfo("css/core.min.css");
        
        CssFiles.Add(fi.PhysicalPath!);
        CssFiles.AddRange(AssetService.GetCssAssets().Select(webPath =>
        {
            var fii = HostEnvironment.WebRootFileProvider.GetFileInfo(webPath.TrimStart('/'));

            return fii.PhysicalPath!;
        }));
        
        Logger.LogInformation("Bundling css files...");
        
        var content = "";

        if (CssFiles.Count > 0)
        {
            var cssFileContents = new List<string>();

            foreach (var cssFile in CssFiles)
            {
                var fileContent = await File.ReadAllTextAsync(cssFile, cancellationToken);
                cssFileContents.Add(fileContent);
            }
            
            content = cssFileContents[0];

            if (cssFileContents.Count > 1)
            {
                var styleSheets = new List<Stylesheet>();

                var parser = new StylesheetParser();

                foreach (var fileContent in cssFileContents)
                {
                    try
                    {
                        var sh = await parser.ParseAsync(fileContent, cancellationToken);
                        styleSheets.Add(sh);
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning("An error occured while parsing css file: {e}", e);
                    }
                }

                var mainStylesheet = styleSheets.First();

                foreach (var stylesheet in styleSheets.Skip(1))
                {
                    // Style
                    foreach (var styleRule in stylesheet.StyleRules)
                    {
                        if (mainStylesheet.StyleRules.Any(x => x.Selector.Text == styleRule.Selector.Text))
                            continue;

                        content += styleRule.ToCss() + "\n";
                    }
                    
                    // Container
                    foreach (var containerRule in stylesheet.ContainerRules)
                    {
                        if (mainStylesheet.ContainerRules.Any(x => x.ConditionText == containerRule.ConditionText))
                            continue;

                        content += containerRule.ToCss() + "\n";
                    }
                    
                    // Import Rule
                    foreach (var importRule in stylesheet.ImportRules)
                    {
                        if (mainStylesheet.ImportRules.Any(x => x.Text == importRule.Text))
                            continue;

                        content += importRule.ToCss() + "\n";
                    }
                    
                    // Media Rules
                    foreach (var mediaRule in stylesheet.MediaRules)
                        content += mediaRule.StylesheetText.Text + "\n";
                    
                    // Page Rules
                    foreach (var pageRule in stylesheet.PageRules)
                    {
                        if (mainStylesheet.PageRules.Any(x => x.SelectorText == pageRule.SelectorText))
                            continue;

                        content += pageRule.ToCss() + "\n";
                    }
                }
            }
        }

        Directory.CreateDirectory(PathBuilder.Dir("storage", "tmp"));
        await File.WriteAllTextAsync(PathBuilder.File("storage", "tmp", "bundle.css"), content, cancellationToken);
        
        Logger.LogInformation("Successfully built css bundle");
    }

    public void AddCssFile(string file) => CssFiles.Add(file);

    //
    public Task StartAsync(CancellationToken cancellationToken)
        => Bundle(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}