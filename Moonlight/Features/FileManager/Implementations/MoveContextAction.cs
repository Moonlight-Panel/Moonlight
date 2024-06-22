
using MoonCore.Blazor.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

namespace Moonlight.Features.FileManager.Implementations;

public class MoveContextAction : IFileManagerContextAction
{
    public string Name => "Move";
    public string Icon => "bx-move";
    public string Color => "info";
    public Func<FileEntry, bool> Filter => _ => true;
    
    public async Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, FileEntry entry, IServiceProvider provider)
    {
        await fileManager.OpenFolderSelect("Select the location to move the item to", async path =>
        {
            var toastService = provider.GetRequiredService<ToastService>();
            
            await access.Move(entry, path + entry.Name);

            await toastService.Success("Successfully moved item");
            await fileManager.View.Refresh();
        });
    }
}