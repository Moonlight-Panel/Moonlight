using Moonlight.App.ApiClients.Wings;
using Moonlight.App.ApiClients.Wings.Resources;
using Moonlight.App.Database.Entities;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Daemon.Resources;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services;

public class NodeService
{
    private readonly WingsApiHelper WingsApiHelper;
    private readonly DaemonApiHelper DaemonApiHelper;
    
    public NodeService(WingsApiHelper wingsApiHelper, DaemonApiHelper daemonApiHelper)
    {
        WingsApiHelper = wingsApiHelper;
        DaemonApiHelper = daemonApiHelper;
    }

    public async Task<SystemStatus> GetStatus(Node node)
    {
        return await WingsApiHelper.Get<SystemStatus>(node, "api/system");
    }

    public async Task<CpuStats> GetCpuStats(Node node)
    {
        return await DaemonApiHelper.Get<CpuStats>(node, "stats/cpu");
    }
    
    public async Task<MemoryStats> GetMemoryStats(Node node)
    {
        return await DaemonApiHelper.Get<MemoryStats>(node, "stats/memory");
    }
    
    public async Task<DiskStats> GetDiskStats(Node node)
    {
        return await DaemonApiHelper.Get<DiskStats>(node, "stats/disk");
    }
    
    public async Task<ContainerStats> GetContainerStats(Node node)
    {
        return await DaemonApiHelper.Get<ContainerStats>(node, "stats/container");
    }

    public async Task<bool> IsHostUp(Node node)
    {
        try
        {
            //TODO: Implement status caching
            var data = await GetStatus(node);

            if (data != null)
                return true;
        }
        catch (Exception)
        {
            // ignored
        }

        return false;
    }
}