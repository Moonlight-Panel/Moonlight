using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Models.Abstractions;

namespace Moonlight.Features.Servers.Services;

public class NodeService
{
    private readonly Dictionary<int, NodeMeta> MetaCache = new();

    public Task UpdateMeta(ServerNode node, Action<NodeMeta> metaAction)
    {
        lock (MetaCache)
        {
            NodeMeta? meta = null;

            if (MetaCache.ContainsKey(node.Id))
                meta = MetaCache[node.Id];

            if (meta == null)
            {
                meta = new();
                MetaCache.Add(node.Id, meta);
            }
            
            metaAction.Invoke(meta);
        }
        
        return Task.CompletedTask;
    }
}