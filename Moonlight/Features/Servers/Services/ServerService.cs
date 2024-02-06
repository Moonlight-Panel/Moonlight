using MoonCore.Attributes;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Extensions;
using Moonlight.Features.Servers.Helpers;
using Moonlight.Features.Servers.Models.Abstractions;

namespace Moonlight.Features.Servers.Services;

[Singleton]
public class ServerService
{
    public readonly MetaCache<ServerMeta> Meta = new();
    public ServerConsoleService Console => ServiceProvider.GetRequiredService<ServerConsoleService>();

    private readonly IServiceProvider ServiceProvider;

    public ServerService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task Sync(Server server)
    {
        using var httpClient = server.CreateHttpClient(ServiceProvider);
        await httpClient.Post($"servers/{server.Id}/sync");
    }

    public async Task SyncDelete(Server server)
    {
        
    }
}