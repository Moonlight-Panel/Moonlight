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
        
        if(!await alertService.YesNo($"Do you really want to delete {entries.Length} item(s)?"))
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