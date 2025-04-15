using ExCSS;
using Microsoft.Extensions.FileProviders;
using MoonCore.Helpers;

namespace Moonlight.ApiServer.Services;

public class BundleGenerationService : IHostedService
{
    private readonly ILogger<BundleGenerationService> Logger;
    private readonly IWebHostEnvironment HostEnvironment;
    private readonly PluginService PluginService;
    private readonly BundleService BundleService;

    public BundleGenerationService(
        ILogger<BundleGenerationService> logger,
        IWebHostEnvironment hostEnvironment,
        BundleService bundleService,
        PluginService pluginService
    )
    {
        Logger = logger;
        HostEnvironment = hostEnvironment;
        BundleService = bundleService;
        PluginService = pluginService;
    }

    private async Task Bundle(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Bundling css files...");

        // Search the physical path for the defined files
        var physicalCssFiles = new List<string>();

        foreach (var cssFile in BundleService.GetCssFiles())
        {
            var fileInfo = HostEnvironment.WebRootFileProvider.GetFileInfo(cssFile);

            if (fileInfo is NotFoundFileInfo || fileInfo.PhysicalPath == null)
            {
                fileInfo = PluginService.WwwRootFileProvider.GetFileInfo(cssFile);

                if (fileInfo is NotFoundFileInfo || fileInfo.PhysicalPath == null)
                {
                    Logger.LogWarning(
                        "Unable to find physical path for the requested css file '{file}'. Make sure its inside a wwwroot folder",
                        cssFile
                    );

                    continue;
                }
            }

            Logger.LogTrace("Discovered css file '{path}' at '{physicalPath}'", cssFile, fileInfo.PhysicalPath);

            physicalCssFiles.Add(fileInfo.PhysicalPath);
        }

        if (physicalCssFiles.Count == 0)
            Logger.LogWarning(
                "No physical paths to css files loaded. The generated bundle will be empty. Unless this is intended by you this is a bug");

        // TODO: Implement cache

        // TODO: File system watcher for development

        var bundleContent = await CreateCssBundle(physicalCssFiles);

        Directory.CreateDirectory(PathBuilder.Dir("storage", "tmp"));
        
        await File.WriteAllTextAsync(PathBuilder.File("storage", "tmp", "bundle.css"), bundleContent,
            cancellationToken);

        Logger.LogInformation("Successfully built css bundle");
    }

    private async Task<string> CreateCssBundle(List<string> physicalPaths)
    {
        if (physicalPaths.Count == 0) // No stylesheets defined => nothing to process
            return "";

        if (physicalPaths.Count == 1) // Only one stylesheet => nothing to process
            return await File.ReadAllTextAsync(physicalPaths[0]);

        // Create bundle by stripping out double declared classes and combining all css files into one bundle
        var parser = new StylesheetParser();
        string? content = null;
        Stylesheet? mainStylesheet = null;
        var additionalStyleSheets = new List<Stylesheet>();

        foreach (var physicalPath in physicalPaths)
        {
            try
            {
                var fileContent = await File.ReadAllTextAsync(physicalPath);
                var stylesheet = await parser.ParseAsync(fileContent);

                // Check if it's the first stylesheet we are loading
                if (mainStylesheet == null || content == null)
                {
                    // Delegate the first stylesheet to be the main one
                    content = fileContent + "\n";
                    mainStylesheet = stylesheet;
                }
                else
                    additionalStyleSheets.Add(stylesheet); // All other stylesheets are to be processed
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while parsing css file: {e}", e);
            }
        }

        // Handle an empty main stylesheet delegation
        if (mainStylesheet == null || content == null)
        {
            Logger.LogError("An unable to delegate main stylesheet. Did every load attempt of an stylesheet fail?");
            return "";
        }

        // Process stylesheets against the main one
        foreach (var stylesheet in additionalStyleSheets)
        {
            // Style
            foreach (var styleRule in stylesheet.StyleRules)
            {
                if (mainStylesheet.StyleRules.Any(x => x.Selector.Text == styleRule.Selector.Text))
                    continue;

                content += styleRule.StylesheetText.Text + "\n";
            }

            // Container
            foreach (var containerRule in stylesheet.ContainerRules)
            {
                if (mainStylesheet.ContainerRules.Any(x => x.ConditionText == containerRule.ConditionText))
                    continue;

                content += containerRule.StylesheetText.Text + "\n";
            }

            // Import Rule
            foreach (var importRule in stylesheet.ImportRules)
            {
                if (mainStylesheet.ImportRules.Any(x => x.Text == importRule.Text))
                    continue;

                content = importRule.StylesheetText.Text + "\n" + content;
            }

            // Media Rules
            foreach (var mediaRule in stylesheet.MediaRules)
                content += mediaRule.StylesheetText.Text + "\n";

            // Page Rules
            foreach (var pageRule in stylesheet.PageRules)
            {
                if (mainStylesheet.PageRules.Any(x => x.SelectorText == pageRule.SelectorText))
                    continue;

                content += pageRule.StylesheetText.Text + "\n";
            }
        }

        return content;
    }

    //
    public Task StartAsync(CancellationToken cancellationToken)
        => Bundle(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}