using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Http.Controllers.Frontend;
using Moonlight.ApiServer.Models;
using Moonlight.Shared.Misc;

namespace Moonlight.ApiServer.Services;

[Scoped]
public class FrontendService
{
    private readonly AppConfiguration Configuration;
    private readonly IWebHostEnvironment WebHostEnvironment;
    private readonly IEnumerable<FrontendConfigurationOption> ConfigurationOptions;
    private readonly IServiceProvider ServiceProvider;
    private readonly DatabaseRepository<Theme> ThemeRepository;

    public FrontendService(
        AppConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        IEnumerable<FrontendConfigurationOption> configurationOptions,
        IServiceProvider serviceProvider,
        DatabaseRepository<Theme> themeRepository
    )
    {
        Configuration = configuration;
        WebHostEnvironment = webHostEnvironment;
        ConfigurationOptions = configurationOptions;
        ServiceProvider = serviceProvider;
        ThemeRepository = themeRepository;
    }

    public Task<FrontendConfiguration> GetConfiguration()
    {
        var configuration = new FrontendConfiguration()
        {
            ApiUrl = Configuration.PublicUrl,
            HostEnvironment = "ApiServer"
        };

        return Task.FromResult(configuration);
    }

    public async Task<string> GenerateIndexHtml() // TODO: Cache
    {
        // Load requested theme
        var theme = await ThemeRepository
            .Get()
            .FirstOrDefaultAsync(x => x.IsEnabled);
        
        // Load configured javascript files
        var scripts = ConfigurationOptions
            .SelectMany(x => x.Scripts)
            .Distinct()
            .ToArray();

        // Load configured css files
        var styles = ConfigurationOptions
            .SelectMany(x => x.Styles)
            .Distinct()
            .ToArray();

        return await ComponentHelper.RenderComponent<FrontendPage>(
            ServiceProvider,
            parameters =>
            {
                parameters["Theme"] = theme!;
                parameters["Styles"] = styles;
                parameters["Scripts"] = scripts;
                parameters["Title"] = "Moonlight"; // TODO: Config
            }
        );
    }

    public async Task<Stream> GenerateZip() // TODO: Rework to be able to extract everything successfully
    {
        // We only allow the access to this function when we are actually hosting the frontend
        if (!Configuration.Frontend.EnableHosting)
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

        // Add frontend.json
        var frontendConfig = await GetConfiguration();
        frontendConfig.HostEnvironment = "Static";
        var frontendJson = JsonSerializer.Serialize(frontendConfig);
        await ArchiveText(zipArchive, "frontend.json", frontendJson);

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