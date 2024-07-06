using MoonCore.Blazor.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

namespace Moonlight.Features.FileManager.Implementations;

public class CreateFileAction : IFileManagerCreateAction
{
    public string Name => "File";
    public string Icon => "bx-file";
    public string Color => "primary";
    
    public async Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, IServiceProvider provider)
    {
        var alertService = provider.GetRequiredService<AlertService>();
        
        await alertService.Text("Create a new file","Enter a name for the new file", async name =>
        {
            if (string.IsNullOrEmpty(name) || name.Contains(".."))
                return;

            await access.CreateFile(name);
        
            // We build a virtual entry here so we dont need to fetch one
            await fileManager.OpenEditor(new()
            {
                Name = name,
                Size = 0,
                IsFile = true,
                IsDirectory = false,
                LastModifiedAt = DateTime.UtcNow
            });
        });
    }
}