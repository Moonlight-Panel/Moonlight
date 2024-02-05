namespace Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

public interface IFileLaunchAccess
{
    public Task<string> GetLaunchUrl();
}