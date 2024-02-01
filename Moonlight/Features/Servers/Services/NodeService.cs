using MoonCore.Attributes;
using Moonlight.Features.Servers.Helpers;
using Moonlight.Features.Servers.Models.Abstractions;

namespace Moonlight.Features.Servers.Services;

[Singleton]
public class NodeService
{
    public readonly MetaCache<NodeMeta> Meta = new();
}