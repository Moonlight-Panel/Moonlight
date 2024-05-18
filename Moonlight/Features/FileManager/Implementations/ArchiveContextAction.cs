using MoonCore.Exceptions;
using MoonCore.Helpers;
using MoonCoreUI.Services;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

namespace Moonlight.Features.FileManager.Implementations;

public class ArchiveContextAction : IFileManagerContextAction
{
    public string Name => "Archive";
    public string Icon => "bxs-archive-in";
    public string Color => "warning";
    public Func<FileEntry, bool> Filter => _ => true;

    public async Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, FileEntry entry,
        IServiceProvider provider)
    {
        var archiveAccess = access.Actions as IArchiveFileActions;

        if (archiveAccess == null)
            throw new DisplayException("This file access does not support archiving");

        var alertService = provider.GetRequiredService<AlertService>();

        var fileName = await alertService.Text("Enter the archive file name", "",
            Formatter.FormatDate(DateTime.UtcNow) + ".tar.gz");

        if (string.IsNullOrEmpty(fileName) || fileName.Contains("..")) // => canceled
            return;

        var toastService = provider.GetRequiredService<ToastService>();

        await toastService.CreateProgress("fileManagerArchive", "Archiving... Please be patient");

        try
        {
            await archiveAccess.Archive(
                access.CurrentDirectory + fileName,
                new[] { access.CurrentDirectory + entry.Name }
            );
            
            await toastService.Success("Successfully created archive");
        }
        catch (Exception e)
        {
            Logger.Warn($"An error occured while archiving item ({entry.Name}):");
            Logger.Warn(e);

            await toastService.Danger("An unknown error occured while creating archive");
        }
        finally
        {
            await toastService.RemoveProgress("fileManagerArchive");
        }

        await fileManager.View.Refresh();
    }
}