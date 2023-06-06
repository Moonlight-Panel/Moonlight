using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moonlight.App.Database.Entities;
using Moonlight.App.Repositories;
using Moonlight.App.Services;

namespace Moonlight.App.Diagnostics.HealthChecks;

public class NodeHealthCheck : IHealthCheck
{
    private readonly Repository<Node> NodeRepository;
    private readonly NodeService NodeService;

    public NodeHealthCheck(Repository<Node> nodeRepository, NodeService nodeService)
    {
        NodeRepository = nodeRepository;
        NodeService = nodeService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var nodes = NodeRepository.Get().ToArray();
        
        var results = new Dictionary<Node, bool>();
        var healthCheckData = new Dictionary<string, object>();
        
        foreach (var node in nodes)
        {
            try
            {
                await NodeService.GetStatus(node);
                
                results.Add(node, true);
            }
            catch (Exception e)
            {
                results.Add(node, false);
                healthCheckData.Add(node.Name, e.ToStringDemystified());
            }
        }

        var offlineNodes = results
            .Where(x => !x.Value)
            .ToArray();

        if (offlineNodes.Length == nodes.Length)
        {
            return HealthCheckResult.Unhealthy("All nodes are offline", null, healthCheckData);
        }
        
        if (offlineNodes.Length == 0)
        {
            return HealthCheckResult.Healthy("All nodes are online");
        }
            
        return HealthCheckResult.Degraded($"{offlineNodes.Length} nodes are offline", null, healthCheckData);
    }
}