using System.IO.Compression;
using MoonCore.Attributes;
using MoonCore.Helpers;
using Moonlight.ApiServer.App.Extensions;
using Moonlight.ApiServer.App.Interfaces;
using Moonlight.ApiServer.App.PluginApi;

namespace Moonlight.ApiServer.App.Services;

[Scoped]
public class DiagnoseService
{
    private readonly PluginService PluginService;
    private readonly IServiceProvider ServiceProvider;
    private readonly ILogger<DiagnoseService> Logger;

    public DiagnoseService(
        PluginService pluginService,
        IServiceProvider serviceProvider,
        ILogger<DiagnoseService> logger)
    {
        PluginService = pluginService;
        ServiceProvider = serviceProvider;
        Logger = logger;
    }

    public async Task<MemoryStream> GenerateReport()
    {
        var memoryStream = new MemoryStream();
        var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true);

        var reporters = PluginService.GetImplementations<IDiagnoseReporter>();

        foreach (var reporter in reporters)
        {
            try
            {
                await reporter.GenerateReport(archive, ServiceProvider);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while calling reporter '{name}' while generating diagnostics report: {e}", reporter.GetType().FullName, e);
            }
        }

        await archive.AddText("exported_at.txt", Formatter.FormatDate(DateTime.UtcNow));
        
        archive.Dispose();

        await memoryStream.FlushAsync();
        memoryStream.Position = 0;
        
        return memoryStream;
    }
}