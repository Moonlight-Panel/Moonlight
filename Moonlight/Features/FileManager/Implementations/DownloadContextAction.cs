using Microsoft.AspNetCore.Components;
using MoonCore.Blazor.Services;
using MoonCore.Helpers;

using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.Services;
using Moonlight.Features.FileManager.UI.Components;

namespace Moonlight.Features.FileManager.Implementations;

public class DownloadContextAction : IFileManagerContextAction
{
    public string Name => "Download";
    public string Icon => "bxs-cloud-download";
    public string Color => "primary";
    public Func<FileEntry, bool> Filter => entry => entry.IsFile;

    public async Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, FileEntry entry, IServiceProvider provider)
    {
        var fileAccessService = provider.GetRequiredService<SharedFileAccessService>();
        var navigation = provider.GetRequiredService<NavigationManager>();
        var toastService = provider.GetRequiredService<ToastService>();
        
        try
        {
            var token = await fileAccessService.GenerateToken(access);
            var url = $"/api/download?token={token}&name={entry.Name}";

            await toastService.Info("Starting download...");
            navigation.NavigateTo(url, true);
        }
        catch (Exception e)
        {
            var logger = provider.GetRequiredService<ILogger<DownloadContextAction>>();
            logger.LogWarning("Unable to start download: {e}", e); ;

            await toastService.Danger("Failed to start download");
        }
    }
}