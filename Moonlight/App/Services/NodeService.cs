using Moonlight.App.ApiClients.Daemon;
using Moonlight.App.ApiClients.Daemon.Requests;
using Moonlight.App.ApiClients.Daemon.Resources;
using Moonlight.App.ApiClients.Wings;
using Moonlight.App.ApiClients.Wings.Resources;
using Moonlight.App.Database.Entities;
using Moonlight.App.Helpers;
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

    public async Task<CpuMetrics> GetCpuMetrics(Node node)
    {
        return await DaemonApiHelper.Get<CpuMetrics>(node, "metrics/cpu");
    }
    
    public async Task<MemoryMetrics> GetMemoryMetrics(Node node)
    {
        return await DaemonApiHelper.Get<MemoryMetrics>(node, "metrics/memory");
    }
    
    public async Task<DiskMetrics> GetDiskMetrics(Node node)
    {
        return await DaemonApiHelper.Get<DiskMetrics>(node, "metrics/disk");
    }
    
    public async Task<SystemMetrics> GetSystemMetrics(Node node)
    {
        return await DaemonApiHelper.Get<SystemMetrics>(node, "metrics/system");
    }
    
    public async Task<DockerMetrics> GetDockerMetrics(Node node)
    {
        return await DaemonApiHelper.Get<DockerMetrics>(node, "metrics/docker");
    }

    public async Task Mount(Node node, string server, string serverPath, string path)
    {
        await DaemonApiHelper.Post(node, "mount", new Mount()
        {
            Server = server,
            ServerPath = serverPath,
            Path = path
        });
    }

    public async Task Unmount(Node node, string path)
    {
        await DaemonApiHelper.Delete(node, "mount", new Unmount()
        {
            Path = path
        });
    }

    public async Task<bool> IsHostUp(Node node)
    {
        try
        {
            await GetStatus(node);

            return true;
        }
        catch (Exception)
        {
            // ignored
        }

        return false;
    }
}