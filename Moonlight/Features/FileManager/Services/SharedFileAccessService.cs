using MoonCore.Attributes;
using Moonlight.Core.Services.Utils;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

namespace Moonlight.Features.FileManager.Services;

[Singleton]
public class SharedFileAccessService
{
    private readonly JwtService JwtService;
    private readonly List<IFileAccess> FileAccesses = new();

    public SharedFileAccessService(JwtService jwtService)
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
        }, "FileAccess", TimeSpan.FromMinutes(6));

        return token;
    }
}