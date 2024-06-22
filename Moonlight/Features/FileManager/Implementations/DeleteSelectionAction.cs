
using MoonCore.Blazor.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.UI.Components;

namespace Moonlight.Features.FileManager.Implementations;

public class DeleteSelectionAction : IFileManagerSelectionAction
{
    public string Name => "Delete";
    public string Color => "danger";
    
    public async Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, FileEntry[] entries, IServiceProvider provider)
    {
        var alertService = provider.GetRequiredService<AlertService>();
        var toastService = provider.GetRequiredService<ToastService>();

        var folderEmoji = "\ud83d\udcc1";
        var fileEmoji = "\ud83d\udcc4";

        var showFolderCount = 3;
        var showFileCount = 6;

        var folderCount = entries.Count(x => x.IsDirectory);
        var fileCount = entries.Count(x => x.IsFile);
        
        // Construct file list
        var fileList = "";

        foreach (var folder in entries.Where(x => x.IsDirectory).Take(showFolderCount))
        {
            fileList += folderEmoji + "  " + folder.Name + "\n";
        }

        if (folderCount > showFolderCount)
            fileList += "And " + (folderCount - showFolderCount) + " more folders... \n\n";
        
        foreach (var file in entries.Where(x => x.IsFile).Take(showFileCount))
        {
            fileList += fileEmoji + "  " + file.Name + "\n";    
        }

        if (fileCount > showFileCount)
            fileList += "And " + (fileCount - showFileCount) + " more files...";


        await alertService.Confirm("Confirm file deletion",
            $"Do you really want to delete {folderCount + fileCount} item(s)? \n\n" + fileList, async () =>
            {
                await toastService.CreateProgress("fileManagerSelectionDelete", "Deleting items");

                foreach (var entry in entries)
                {
                    await toastService.UpdateProgress("fileManagerSelectionDelete", $"Deleting '{entry.Name}'");

                    await access.Delete(entry);
                }

                await toastService.DeleteProgress("fileManagerSelectionDelete");

                await toastService.Success("Successfully deleted selection");
                await fileManager.View.Refresh();
            });
    }
}