using ExCSS;
using Microsoft.Extensions.FileProviders;
using MoonCore.Helpers;

namespace Moonlight.ApiServer.Services;

public class BundleGenerationService : IHostedService
{
    private readonly ILogger<BundleGenerationService> Logger;
    private readonly IWebHostEnvironment HostEnvironment;
    private readonly BundleService BundleService;

    public BundleGenerationService(
        ILogger<BundleGenerationService> logger,
        IWebHostEnvironment hostEnvironment,
        BundleService bundleService
    )
    {
        Logger = logger;
        HostEnvironment = hostEnvironment;
        BundleService = bundleService;
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
                Logger.LogWarning(
                    "Unable to find physical path for the requested css file '{file}'. Make sure its inside a wwwroot folder",
                    cssFile
                );
                
                continue;
            }

            physicalCssFiles.Add(fileInfo.PhysicalPath);
        }

        // TODO: Implement cache

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
        var content = "";
        var styleSheets = new List<Stylesheet>();
        var parser = new StylesheetParser();

        foreach (var fileContent in physicalPaths)
        {
            try
            {
                var sh = await parser.ParseAsync(fileContent);
                styleSheets.Add(sh);
            }
            catch (Exception e)
            {
                Logger.LogWarning("An error occured while parsing css file: {e}", e);
            }
        }

        // Delegate the first stylesheet as the main stylesheet
        var mainStylesheet = styleSheets.First();

        foreach (var stylesheet in styleSheets.Skip(1)) // Process all stylesheets expect the first (main) one
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

        return content;
    }

    //
    public Task StartAsync(CancellationToken cancellationToken)
        => Bundle(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}