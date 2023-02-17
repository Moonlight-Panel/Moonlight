using Moonlight.App.Repositories;

namespace Moonlight.App.Services;

public class NodeService
{
    private readonly NodeRepository NodeRepository;
    
    public NodeService(NodeRepository nodeRepository)
    {
        NodeRepository = nodeRepository;
    }
}