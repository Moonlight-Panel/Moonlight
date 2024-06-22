using MoonCore.Blazor.Services;
using MoonCore.Exceptions;
using MoonCore.Helpers;

using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

namespace Moonlight.Features.FileManager.Implementations;

public class ExtractContextAction : IFileManagerContextAction
{
    public string Name => "Extract";
    public string Icon => "bxs-archive-out";
    public string Color => "warning";
    public Func<FileEntry, bool> Filter => entry => entry.IsFile && entry.Name.EndsWith(".tar.gz");
    public async Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, FileEntry entry, IServiceProvider provider)
    {
        var archiveAccess = access.Actions as IArchiveFileActions;

        if (archiveAccess == null)
            throw new DisplayException("This file access does not support archiving");

        await fileManager.OpenFolderSelect("Select where you want to extract the content of the archive", async destination =>
        {
            var toastService = provider.GetRequiredService<ToastService>();

            await toastService.CreateProgress("fileManagerExtract", "Extracting... Please be patient");

            try
            {
                await archiveAccess.Extract(
                    access.CurrentDirectory + entry.Name,
                    destination
                );
            
                await toastService.Success("Successfully extracted archive");
            }
            catch (Exception e)
            {
                var logger = provider.GetRequiredService<ILogger<ExtractContextAction>>();
                logger.LogWarning("An error occured while extracting archive ({name}): {e}", entry.Name, e);

                await toastService.Danger("An unknown error occured while extracting archive");
            }
            finally
            {
                await toastService.DeleteProgress("fileManagerExtract");
            }

            await fileManager.View.Refresh();
        });
    }
}