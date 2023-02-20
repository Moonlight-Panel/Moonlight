using Moonlight.App.Database.Entities;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Wings.Resources;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services;

public class NodeService
{
    private readonly NodeRepository NodeRepository;
    private readonly WingsApiHelper WingsApiHelper;
    
    public NodeService(NodeRepository nodeRepository, WingsApiHelper wingsApiHelper)
    {
        NodeRepository = nodeRepository;
        WingsApiHelper = wingsApiHelper;
    }

    public async Task<SystemStatus> GetStatus(Node node)
    {
        return await WingsApiHelper.Get<SystemStatus>(node, "api/system");
    }
}