using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.UI.NewFileManager;

namespace Moonlight.Features.FileManager.Interfaces;

public interface IFileManagerAction
{
    public string Name { get; }
    public string Icon { get; }
    public string Color { get; }
    public Func<FileEntry, bool> Filter { get; }

    public Task Execute(BaseFileAccess access, FileView view, FileEntry entry, IServiceProvider provider);
}