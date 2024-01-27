using Moonlight.Features.Servers.Helpers;
using Moonlight.Features.Servers.Models.Abstractions;

namespace Moonlight.Features.Servers.Services;

public class NodeService
{
    public readonly MetaCache<NodeMeta> Meta = new();
}