using MoonCore.Blazor.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.UI.Components;

namespace Moonlight.Features.FileManager.Implementations;

public class CreateFolderAction : IFileManagerCreateAction
{
    public string Name => "Folder";
    public string Icon => "bx-folder";
    public string Color => "primary";
    
    public async Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, IServiceProvider provider)
    {
        var alertService = provider.GetRequiredService<AlertService>();
        var toastService = provider.GetRequiredService<ToastService>();
        
        await alertService.Text("Create a new folder", "Enter a name for the new directory", async name =>
        {
            if (string.IsNullOrEmpty(name) || name.Contains(".."))
                return;

            await access.CreateDirectory(name);

            await toastService.Success("Successfully created directory");
            await fileManager.View.Refresh();
        });
    }
}