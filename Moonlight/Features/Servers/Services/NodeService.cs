using MoonCore.Attributes;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Extensions;
using Moonlight.Features.Servers.Helpers;
using Moonlight.Features.Servers.Models.Abstractions;

namespace Moonlight.Features.Servers.Services;

[Singleton]
public class NodeService
{
    public readonly MetaCache<NodeMeta> Meta = new();

    private readonly IServiceProvider ServiceProvider;

    public NodeService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task Boot(ServerNode node)
    {
        using var httpClient = node.CreateHttpClient();
        await httpClient.Post("system/boot");
    }
}