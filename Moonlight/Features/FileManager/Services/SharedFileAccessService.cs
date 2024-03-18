using MoonCore.Attributes;
using MoonCore.Services;
using Moonlight.Core.Services;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.FileManager.Models.Enums;

namespace Moonlight.Features.FileManager.Services;

[Singleton]
public class SharedFileAccessService
{
    private readonly JwtService<FileManagerJwtType> JwtService;
    private readonly List<IFileAccess> FileAccesses = new();

    public SharedFileAccessService(JwtService<FileManagerJwtType> jwtService)
    {
        JwtService = jwtService;
    }

    public Task<int> Register(IFileAccess fileAccess)
    {
        lock (FileAccesses)
        {
            if(!FileAccesses.Contains(fileAccess))
                FileAccesses.Add(fileAccess);
        }

        return Task.FromResult(fileAccess.GetHashCode());
    }

    public Task Unregister(IFileAccess fileAccess)
    {
        lock (FileAccesses)
        {
            if (FileAccesses.Contains(fileAccess))
                FileAccesses.Remove(fileAccess);
        }
        
        return Task.CompletedTask;
    }

    public Task<IFileAccess?> Get(int id)
    {
        lock (FileAccesses)
        {
            var fileAccess = FileAccesses.FirstOrDefault(x => x.GetHashCode() == id);

            if (fileAccess == null)
                return Task.FromResult<IFileAccess?>(null);
            
            return Task.FromResult<IFileAccess?>(fileAccess.Clone());
        }
    }

    public async Task<string> GenerateToken(IFileAccess fileAccess)
    {
        var token = await JwtService.Create(data =>
        {
            data.Add("FileAccessId", fileAccess.GetHashCode().ToString());
        }, FileManagerJwtType.FileAccess, TimeSpan.FromMinutes(6));

        return token;
    }
}