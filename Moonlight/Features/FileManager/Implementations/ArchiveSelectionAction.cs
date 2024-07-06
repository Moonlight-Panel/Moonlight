using MoonCore.Blazor.Services;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.Features.FileManager.Interfaces;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

namespace Moonlight.Features.FileManager.Implementations;

public class ArchiveSelectionAction : IFileManagerSelectionAction
{
    public string Name => "Archive";
    public string Color => "primary";

    public async Task Execute(BaseFileAccess access, UI.Components.FileManager fileManager, FileEntry[] entries,
        IServiceProvider provider)
    {
        var archiveAccess = access.Actions as IArchiveFileActions;

        if (archiveAccess == null)
            throw new DisplayException("This file access does not support archiving");

        var alertService = provider.GetRequiredService<AlertService>();

        await alertService.Text("Create an archive", "Enter the archive file name", async fileName =>
            {
                if (string.IsNullOrEmpty(fileName) || fileName.Contains("..")) // => canceled
                    return;

                var toastService = provider.GetRequiredService<ToastService>();

                await toastService.CreateProgress("fileManagerArchive", "Archiving... Please be patient");

                try
                {
                    await archiveAccess.Archive(
                        access.CurrentDirectory + fileName,
                        entries.Select(x => access.CurrentDirectory + x.Name).ToArray()
                    );

                    await toastService.Success("Successfully created archive");
                }
                catch (Exception e)
                {
                    var logger = provider.GetRequiredService<ILogger<ArchiveSelectionAction>>();
                    logger.LogWarning("An error occured while archiving items ({lenght}): {e}", entries.Length, e);

                    await toastService.Danger("An unknown error occured while creating archive");
                }
                finally
                {
                    await toastService.DeleteProgress("fileManagerArchive");
                }
            },
            Formatter.FormatDate(DateTime.UtcNow) + ".tar.gz");
    }
}