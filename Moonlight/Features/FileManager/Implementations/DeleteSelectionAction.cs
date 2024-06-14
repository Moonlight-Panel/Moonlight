using MoonCoreUI.Services;
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
        
        // Construct file list
        var fileList = "";

        foreach (var folder in entries.Where(x => x.IsDirectory).Take(showFolderCount))
        {
            fileList += folderEmoji + "  " + folder.Name + "\n";
        }

        if (entries.Where(x => x.IsDirectory).ToArray().Length > showFolderCount)
            fileList += "And " + (entries.Where(x => x.IsDirectory).ToArray().Length - showFolderCount) + " more folders... \n\n";
        
        foreach (var file in entries.Where(x => x.IsFile).Take(showFileCount))
        {
            fileList += fileEmoji + "  " + file.Name + "\n";    
        }

        if (entries.Where(x => x.IsFile).ToArray().Length > showFileCount)
            fileList += "And " + (entries.Where(x => x.IsFile).ToArray().Length - showFileCount) + " more files...";
        
        
        if(!await alertService.YesNo($"Do you really want to delete {entries.Length} item(s)? \n\n" + fileList))
            return;

        await toastService.CreateProgress("fileManagerSelectionDelete", "Deleting items");

        foreach (var entry in entries)
        {
            await toastService.ModifyProgress("fileManagerSelectionDelete", $"Deleting '{entry.Name}'");

            await access.Delete(entry);
        }

        await toastService.RemoveProgress("fileManagerSelectionDelete");

        await toastService.Success("Successfully deleted selection");
        await fileManager.View.Refresh();
    }
}