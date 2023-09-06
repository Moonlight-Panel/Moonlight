using Moonlight.App.Helpers;
using Octokit;

namespace Moonlight.App.Services.Files;

public class StorageService
{
    public async Task EnsureCreated()
    {
        Directory.CreateDirectory(PathBuilder.Dir("storage", "uploads"));
        Directory.CreateDirectory(PathBuilder.Dir("storage", "configs"));
        Directory.CreateDirectory(PathBuilder.Dir("storage", "resources"));
        Directory.CreateDirectory(PathBuilder.Dir("storage", "backups"));
        Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));
        Directory.CreateDirectory(PathBuilder.Dir("storage", "plugins"));
        Directory.CreateDirectory(PathBuilder.Dir("storage", "certs"));

        await UpdateResources();

        return;
        if (IsEmpty(PathBuilder.Dir("storage", "resources")))
        {
            Logger.Info("Default resources not found. Copying default resources");

            CopyFilesRecursively(
                PathBuilder.Dir("defaultstorage", "resources"),
                PathBuilder.Dir("storage", "resources")
            );
        }

        if (IsEmpty(PathBuilder.Dir("storage", "configs")))
        {
            Logger.Info("Default configs not found. Copying default configs");

            CopyFilesRecursively(
                PathBuilder.Dir("defaultstorage", "configs"),
                PathBuilder.Dir("storage", "configs")
            );
        }
    }

    private async Task UpdateResources()
    {
        try
        {
            Logger.Info("Checking resources");

            var client = new GitHubClient(
                new ProductHeaderValue("Moonlight-Panel"));

            var user = "Moonlight-Panel";
            var repo = "Resources";
            var resourcesDir = PathBuilder.Dir("storage", "resources");

            async Task CopyDirectory(string dirPath, string localDir)
            {
                IReadOnlyList<RepositoryContent> contents;

                if (string.IsNullOrEmpty(dirPath))
                    contents = await client.Repository.Content.GetAllContents(user, repo);
                else
                    contents = await client.Repository.Content.GetAllContents(user, repo, dirPath);

                foreach (var content in contents)
                {
                    string localPath = Path.Combine(localDir, content.Name);

                    if (content.Type == ContentType.File)
                    {
                        if (content.Name.EndsWith(".gitattributes"))
                            continue;

                        if (File.Exists(localPath) && !content.Name.EndsWith(".lang"))
                            continue;

                        if (content.Name.EndsWith(".lang") && File.Exists(localPath) &&
                            new FileInfo(localPath).Length == content.Size)
                            continue;

                        var fileContent = await client.Repository.Content.GetRawContent(user, repo, content.Path);
                        Directory.CreateDirectory(localDir); // Ensure the directory exists
                        await File.WriteAllBytesAsync(localPath, fileContent);

                        Logger.Debug($"Synced file '{content.Path}'");
                    }
                    else if (content.Type == ContentType.Dir)
                    {
                        await CopyDirectory(content.Path, localPath);
                    }
                }
            }

            await CopyDirectory("", resourcesDir);
        }
        catch (RateLimitExceededException)
        {
            Logger.Warn("Unable to sync resources due to your ip being rate-limited by github");
        }
        catch (Exception e)
        {
            Logger.Warn("Unable to sync resources");
            Logger.Warn(e);
        }
    }

    private bool IsEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
}