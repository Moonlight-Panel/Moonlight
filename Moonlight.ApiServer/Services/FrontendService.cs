using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.Shared.Misc;

namespace Moonlight.ApiServer.Services;

[Scoped]
public class FrontendService
{
    private readonly AppConfiguration Configuration;
    private readonly PluginService PluginService;
    private readonly IWebHostEnvironment WebHostEnvironment;

    public FrontendService(
        AppConfiguration configuration,
        PluginService pluginService,
        IWebHostEnvironment webHostEnvironment
    )
    {
        Configuration = configuration;
        PluginService = pluginService;
        WebHostEnvironment = webHostEnvironment;
    }

    public async Task<FrontendConfiguration> GetConfiguration()
    {
        var configuration = new FrontendConfiguration()
        {
            Title = "Moonlight",
            ApiUrl = Configuration.PublicUrl,
            HostEnvironment = "ApiServer"
        };

        // Load theme.json if it exists
        var themePath = Path.Combine("storage", "theme.json");

        if (File.Exists(themePath))
        {
            var variablesJson = await File.ReadAllTextAsync(themePath);

            configuration.Theme.Variables = JsonSerializer
                .Deserialize<Dictionary<string, string>>(variablesJson) ?? new();
        }

        // Collect assemblies for the 'client' section
        configuration.Assemblies = PluginService
            .GetAssemblies("client")
            .Keys
            .ToArray();

        // Collect scripts to execute
        configuration.Scripts = PluginService
            .LoadedPlugins
            .Keys
            .SelectMany(x => x.Scripts)
            .ToArray();

        // Collect styles
        var styles = new List<string>();

        styles.AddRange(
            PluginService
                .LoadedPlugins
                .Keys
                .SelectMany(x => x.Styles)
        );

        // Add bundle css
        styles.Add("css/bundle.min.css");

        configuration.Styles = styles.ToArray();

        return configuration;
    }

    public async Task<Stream> GenerateZip()
    {
        // We only allow the access to this function when we are actually hosting the frontend
        if (!Configuration.Client.Enable)
            throw new HttpApiException("The hosting of the wasm client has been disabled", 400);

        // Load and check wasm path
        var wasmMainFile = WebHostEnvironment.WebRootFileProvider.GetFileInfo("index.html");

        if (wasmMainFile is NotFoundFileInfo || string.IsNullOrEmpty(wasmMainFile.PhysicalPath))
            throw new HttpApiException("Unable to find wasm location", 500);

        var wasmPath = Path.GetDirectoryName(wasmMainFile.PhysicalPath)! + "/";

        // Load and check the blazor framework files
        var blazorFile = WebHostEnvironment.WebRootFileProvider.GetFileInfo("_framework/blazor.webassembly.js");

        if (blazorFile is NotFoundFileInfo || string.IsNullOrEmpty(blazorFile.PhysicalPath))
            throw new HttpApiException("Unable to find blazor location", 500);

        var blazorPath = Path.GetDirectoryName(blazorFile.PhysicalPath)! + "/";

        // Create zip
        var memoryStream = new MemoryStream();
        var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);

        // Add wasm application
        await ArchiveFsItem(zipArchive, wasmPath, wasmPath);

        // Add blazor files
        await ArchiveFsItem(zipArchive, blazorPath, blazorPath, "_framework/");

        // Add bundle.css
        var bundleContent = await File.ReadAllBytesAsync(Path.Combine("storage", "tmp", "bundle.css"));
        await ArchiveBytes(zipArchive, "css/bundle.css", bundleContent);

        // Add frontend.json
        var frontendConfig = await GetConfiguration();
        frontendConfig.HostEnvironment = "Static";
        var frontendJson = JsonSerializer.Serialize(frontendConfig);
        await ArchiveText(zipArchive, "frontend.json", frontendJson);

        // Add plugin wwwroot files
        foreach (var pluginPath in PluginService.LoadedPlugins.Values)
        {
            var wwwRootPluginPath = Path.Combine(pluginPath, "wwwroot/");

            if (!Directory.Exists(wwwRootPluginPath))
                continue;

            await ArchiveFsItem(zipArchive, wwwRootPluginPath, wwwRootPluginPath);
        }
        
        // Add plugin assemblies for client to the zip file
        var assembliesMap = PluginService.GetAssemblies("client");

        foreach (var assemblyName in assembliesMap.Keys)
        {
            var path = assembliesMap[assemblyName];
            
            await ArchiveFsItem(
                zipArchive,
                path,
                path,
                $"plugins/{assemblyName}"
            );
        }

        // Finish zip archive and reset stream so the code calling this function can process it
        zipArchive.Dispose();
        await memoryStream.FlushAsync();
        memoryStream.Position = 0;

        return memoryStream;
    }

    private async Task ArchiveFsItem(ZipArchive archive, string path, string prefixToRemove, string prefixToAdd = "")
    {
        if (File.Exists(path))
        {
            var entryName = prefixToAdd + Formatter.ReplaceStart(path, prefixToRemove, "");

            var entry = archive.CreateEntry(entryName);

            await using var entryStream = entry.Open();
            await using var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            await fileStream.CopyToAsync(entryStream);
            await entryStream.FlushAsync();

            entryStream.Close();
        }
        else
        {
            foreach (var directoryItem in Directory.EnumerateFileSystemEntries(path))
                await ArchiveFsItem(archive, directoryItem, prefixToRemove, prefixToAdd);
        }
    }

    private async Task ArchiveText(ZipArchive archive, string path, string content)
    {
        var data = Encoding.UTF8.GetBytes(content);
        await ArchiveBytes(archive, path, data);
    }

    private async Task ArchiveBytes(ZipArchive archive, string path, byte[] bytes)
    {
        var entry = archive.CreateEntry(path);
        await using var dataStream = entry.Open();

        await dataStream.WriteAsync(bytes);
        await dataStream.FlushAsync();
    }
}