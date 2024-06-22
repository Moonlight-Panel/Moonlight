
using MoonCore.Blazor.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.UI.Components;

namespace Moonlight.Features.FileManager.Implementations;

public class RenameContextAction : IFileManagerContextAction
{
    public string Name => "Rename";
    public string Icon => "bxs-rename";
    public string Color => "info";
    public Func<FileEntry, bool> Filter => _ => true;
    
    public async Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, FileEntry entry, IServiceProvider provider)
    {
        var alertService = provider.GetRequiredService<AlertService>();
        var toastService = provider.GetRequiredService<ToastService>();
        
        await alertService.Text("Rename file" , $"Enter a new name for '{entry.Name}'", async newName =>
        {
            if (string.IsNullOrEmpty(newName))
                return;

            await access.Rename(entry.Name, newName);

            await fileManager.View.Refresh();
            await toastService.Success("Successfully renamed file");
        }, entry.Name);
    }
}