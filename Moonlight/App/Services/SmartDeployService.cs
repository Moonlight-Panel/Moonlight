using Moonlight.App.Database.Entities;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services;

public class SmartDeployService
{
    private readonly NodeRepository NodeRepository;
    private readonly Repository<CloudPanel> CloudPanelRepository;
    private readonly WebSpaceService WebSpaceService;
    private readonly NodeService NodeService;

    public SmartDeployService(
        NodeRepository nodeRepository,
        NodeService nodeService,
        WebSpaceService webSpaceService,
        Repository<CloudPanel> cloudPanelRepository)
    {
        NodeRepository = nodeRepository;
        NodeService = nodeService;
        WebSpaceService = webSpaceService;
        CloudPanelRepository = cloudPanelRepository;
    }

    public async Task<Node?> GetNode()
    {
        var data = new Dictionary<Node, double>();

        foreach (var node in NodeRepository.Get().ToArray())
        {
            var u = await GetUsageScore(node);
            
            if(u != 0)
                data.Add(node, u);
        }

        if (!data.Any())
            return null;

        return data.MaxBy(x => x.Value).Key;
    }

    public async Task<CloudPanel?> GetCloudPanel()
    {
        var result = new List<CloudPanel>();
        
        foreach (var cloudPanel in CloudPanelRepository.Get().ToArray())
        {
            if (await WebSpaceService.IsHostUp(cloudPanel))
                result.Add(cloudPanel);
        }

        return result.FirstOrDefault();
    }

    private async Task<double> GetUsageScore(Node node)
    {
        var score = 0;

        try
        {
            var cpuMetrics = await NodeService.GetCpuMetrics(node);
            var memoryMetrics = await NodeService.GetMemoryMetrics(node);
            var diskMetrics = await NodeService.GetDiskMetrics(node);
            
            var cpuWeight = 0.5; // Weight of CPU usage in the final score
            var memoryWeight = 0.3; // Weight of memory usage in the final score
            var diskSpaceWeight = 0.2; // Weight of free disk space in the final score

            var cpuScore = (1 - cpuMetrics.CpuUsage) * cpuWeight; // CPU score is based on the inverse of CPU usage
            var memoryScore = (1 - (memoryMetrics.Used / 1024)) * memoryWeight; // Memory score is based on the percentage of free memory
            var diskSpaceScore = (double) (diskMetrics.Total - diskMetrics.Used) / 1000000000 * diskSpaceWeight; // Disk space score is based on the amount of free disk space in GB

            var finalScore = cpuScore + memoryScore + diskSpaceScore;

            return finalScore;
        }
        catch (Exception e)
        {
            // ignored
        }

        return score;
    }
}