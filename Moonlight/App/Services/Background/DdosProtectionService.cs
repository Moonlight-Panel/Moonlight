using Moonlight.App.ApiClients.Daemon;
using Moonlight.App.Database.Entities;
using Moonlight.App.Events;
using Moonlight.App.Helpers;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Background;

public class DdosProtectionService
{
    private readonly IServiceScopeFactory ServiceScopeFactory;

    public DdosProtectionService(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;

        Task.Run(UnBlocker);
    }

    private async Task UnBlocker()
    {
        var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        
        while (true)
        {
            using var scope = ServiceScopeFactory.CreateScope();
            var blocklistIpRepo = scope.ServiceProvider.GetRequiredService<Repository<BlocklistIp>>();
            
            var ips = blocklistIpRepo
                .Get()
                .ToArray();

            foreach (var ip in ips)
            {
                if (DateTime.UtcNow > ip.ExpiresAt)
                {
                    blocklistIpRepo.Delete(ip);
                }
            }

            var newCount = blocklistIpRepo
                .Get()
                .Count();

            if (newCount != ips.Length)
            {
                await RebuildNodeFirewalls();
            }

            await periodicTimer.WaitForNextTickAsync();
        }
    }

    public async Task RebuildNodeFirewalls()
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var blocklistIpRepo = scope.ServiceProvider.GetRequiredService<Repository<BlocklistIp>>();
        var nodeRepo = scope.ServiceProvider.GetRequiredService<Repository<Node>>();
        var nodeService = scope.ServiceProvider.GetRequiredService<NodeService>();

        var ips = blocklistIpRepo
            .Get()
            .Select(x => x.Ip)
            .ToArray();
        
        foreach (var node in nodeRepo.Get().ToArray())
        {
            try
            {
                await nodeService.RebuildFirewall(node, ips);
            }
            catch (Exception e)
            {
                Logger.Warn($"Error rebuilding firewall on node {node.Name}");
                Logger.Warn(e);
            }
        }
    }

    public async Task ProcessDdosSignal(string ip, long packets)
    {
        using var scope = ServiceScopeFactory.CreateScope();

        var blocklistRepo = scope.ServiceProvider.GetRequiredService<Repository<BlocklistIp>>();
        var whitelistRepo = scope.ServiceProvider.GetRequiredService<Repository<WhitelistIp>>();

        var whitelistIps = whitelistRepo.Get().ToArray();
        
        if(whitelistIps.Any(x => x.Ip == ip))
            return;

        var blocklistIps = blocklistRepo.Get().ToArray();
        
        if(blocklistIps.Any(x => x.Ip == ip))
            return;

        await BlocklistIp(ip, packets);
    }

    public async Task BlocklistIp(string ip, long packets)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var blocklistRepo = scope.ServiceProvider.GetRequiredService<Repository<BlocklistIp>>();
        var configService = scope.ServiceProvider.GetRequiredService<ConfigService>();
        var eventSystem = scope.ServiceProvider.GetRequiredService<EventSystem>();
        
        var blocklistIp = blocklistRepo.Add(new()
        {
            Ip = ip,
            Packets = packets,
            ExpiresAt = DateTime.UtcNow.AddMinutes(configService.Get().Moonlight.Security.BlockIpDuration),
            CreatedAt = DateTime.UtcNow
        });

        await RebuildNodeFirewalls();
        await eventSystem.Emit("ddos.add", blocklistIp);
    }

    public async Task UnBlocklistIp(string ip)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var blocklistRepo = scope.ServiceProvider.GetRequiredService<Repository<BlocklistIp>>();

        var blocklist = blocklistRepo.Get().First(x => x.Ip == ip);
        blocklistRepo.Delete(blocklist);

        await RebuildNodeFirewalls();
    }
}