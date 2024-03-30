using Microsoft.AspNetCore.Components;
using MoonCore.Helpers;
using MoonCoreUI.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.Services;
using Moonlight.Features.FileManager.UI.NewFileManager;

namespace Moonlight.Features.FileManager.Implementations;

public class DownloadAction : IFileManagerAction
{
    public string Name => "Download";
    public string Icon => "bxs-cloud-download";
    public string Color => "primary";
    public Func<FileEntry, bool> Filter => entry => entry.IsFile;

    public async Task Execute(BaseFileAccess access, FileView view, FileEntry entry, IServiceProvider serviceProvider)
    {
        var fileAccessService = serviceProvider.GetRequiredService<SharedFileAccessService>();
        var navigation = serviceProvider.GetRequiredService<NavigationManager>();
        var toastService = serviceProvider.GetRequiredService<ToastService>();
        
        try
        {
            var token = await fileAccessService.GenerateToken(access);
            var url = $"/api/download?token={token}&name={entry.Name}";

            await toastService.Info("Starting download...");
            navigation.NavigateTo(url, true);
        }
        catch (Exception e)
        {
            Logger.Warn("Unable to start download");
            Logger.Warn(e);

            await toastService.Danger("Failed to start download");
        }
    }
}