using MoonCoreUI.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.UI.NewFileManager;

namespace Moonlight.Features.FileManager.Implementations;

public class CreateFolderAction : IFileManagerCreateAction
{
    public string Name => "Folder";
    public string Icon => "bx-folder";
    public string Color => "primary";
    
    public async Task Execute(BaseFileAccess access, FileView view, IServiceProvider provider)
    {
        var alertService = provider.GetRequiredService<AlertService>();
        var toastService = provider.GetRequiredService<ToastService>();
        
        var name = await alertService.Text("Enter a name for the new directory");

        if (string.IsNullOrEmpty(name) || name.Contains(".."))
            return;

        await access.CreateDirectory(name);

        await toastService.Success("Successfully created directory");
        await view.Refresh();
    }
}