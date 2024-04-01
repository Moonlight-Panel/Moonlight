using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.UI.Components;

namespace Moonlight.Features.FileManager.Interfaces;

public interface IFileManagerCreateAction
{
    public string Name { get; }
    public string Icon { get; }
    public string Color { get; }
    
    public Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, IServiceProvider provider);
}