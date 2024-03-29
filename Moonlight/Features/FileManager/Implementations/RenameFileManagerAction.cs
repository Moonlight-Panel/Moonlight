using MoonCoreUI.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.UI.NewFileManager;

namespace Moonlight.Features.FileManager.Implementations;

public class RenameFileManagerAction : IFileManagerAction
{
    public string Name => "Rename";
    public string Icon => "bxs-rename";
    public string Color => "info";
    public Func<FileEntry, bool> Filter => _ => true;
    
    public async Task Execute(BaseFileAccess access, FileView view, FileEntry entry, IServiceProvider provider)
    {
        var alertService = provider.GetRequiredService<AlertService>();
        var toastService = provider.GetRequiredService<ToastService>();
        
        var newName = await alertService.Text($"Enter a new name for '{entry.Name}'", "", entry.Name);

        if (string.IsNullOrEmpty(newName))
            return;

        await access.Rename(entry.Name, newName);

        await view.Refresh();
        await toastService.Success("Successfully renamed file");
    }
}