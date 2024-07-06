
using MoonCore.Blazor.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.UI.Components;

namespace Moonlight.Features.FileManager.Implementations;

public class DeleteContextAction : IFileManagerContextAction
{
    public string Name => "Delete";
    public string Icon => "bxs-trash";
    public string Color => "danger";
    public Func<FileEntry, bool> Filter => _ => true;

    public async Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, FileEntry entry, IServiceProvider serviceProvider)
    {
        await access.Delete(entry);

        await fileManager.View.Refresh();

        var toastService = serviceProvider.GetRequiredService<ToastService>();
        await toastService.Success("Successfully deleted item");
    }
}