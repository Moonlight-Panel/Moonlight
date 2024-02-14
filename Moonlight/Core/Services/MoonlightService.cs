using System.IO.Compression;
using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Event;
using Moonlight.Core.Extensions;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Theming.Services;
using Newtonsoft.Json;

namespace Moonlight.Core.Services;

[Singleton]
public class MoonlightService // This service can be used to perform strictly panel specific actions
{
    private readonly ConfigService<ConfigV1> ConfigService;
    private readonly IServiceProvider ServiceProvider;

    public WebApplication Application { get; set; } // Do NOT modify using a plugin
    public string LogPath { get; set; } // Do NOT modify using a plugin
    public ThemeService Theme => ServiceProvider.GetRequiredService<ThemeService>();

    public MoonlightService(ConfigService<ConfigV1> configService, IServiceProvider serviceProvider)
    {
        ConfigService = configService;
        ServiceProvider = serviceProvider;
    }

    public async Task Restart()
    {
        Logger.Info("Restarting moonlight");

        // Notify all users that this instance will restart
        await Events.OnMoonlightRestart.InvokeAsync();
        await Task.Delay(TimeSpan.FromSeconds(3));

        await Application.StopAsync();
    }

    public async Task<byte[]> GenerateDiagnoseReport()
    {
        var scope = ServiceProvider.CreateScope();

        // Prepare zip file
        var memoryStream = new MemoryStream();
        var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);

        // Add current log
        // We need to open the file this way because we need to specify the access and share mode directly
        // in order to read from a file which is currently written to
        var fs = File.Open(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var sr = new StreamReader(fs);
        var log = await sr.ReadToEndAsync();
        sr.Close();
        fs.Close();

        await zip.AddFromText("log.txt", log);

        // Add node config
        var nodeRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerNode>>();
        var nodes = nodeRepo.Get().ToArray();

        foreach (var node in nodes)
        {
            // Remove sensitive data
            node.Token = string.IsNullOrEmpty(node.Token) ? "IS EMPTY" : "IS NOT EMPTY";
        }

        var nodesJson = JsonConvert.SerializeObject(nodes, Formatting.Indented);
        await zip.AddFromText("nodes.json", nodesJson);

        // Add config
        var configJson = ConfigService.GetDiagnosticJson();
        await zip.AddFromText("config.json", configJson);

        // Make a list of plugins
        var pluginService = scope.ServiceProvider.GetRequiredService<PluginService>();
        var plugins = await pluginService.GetLoadedPlugins();
        var pluginList = "Installed plugins:\n";

        foreach (var plugin in plugins)
        {
            var assembly = plugin.GetType().Assembly;
            pluginList += $"{assembly.FullName} ({assembly.Location})\n";
        }

        await zip.AddFromText("pluginList.txt", pluginList);

        // Add more information here

        // Finalize file
        zip.Dispose();
        memoryStream.Close();
        var data = memoryStream.ToArray();

        return data;
    }
}