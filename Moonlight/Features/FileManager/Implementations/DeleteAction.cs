using MoonCoreUI.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.UI.NewFileManager;

namespace Moonlight.Features.FileManager.Implementations;

public class DeleteAction : IFileManagerAction
{
    public string Name => "Delete";
    public string Icon => "bxs-trash";
    public string Color => "danger";
    public Func<FileEntry, bool> Filter => _ => true;

    public async Task Execute(BaseFileAccess access, FileView view, FileEntry entry, IServiceProvider serviceProvider)
    {
        await access.Delete(entry);

        await view.Refresh();

        var toastService = serviceProvider.GetRequiredService<ToastService>();
        await toastService.Success("Successfully deleted item");
    }
}