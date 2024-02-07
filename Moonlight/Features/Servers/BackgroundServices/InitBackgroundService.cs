using System.Net.Sockets;
using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Helpers;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Services;
using BackgroundService = MoonCore.Abstractions.BackgroundService;

namespace Moonlight.Features.Servers.BackgroundServices;

[BackgroundService]
public class InitBackgroundService : BackgroundService
{
    private readonly IServiceProvider ServiceProvider;

    public InitBackgroundService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public override async Task Run()
    {
        Logger.Info("Booting all nodes");
        
        using var scope = ServiceProvider.CreateScope();

        var nodeRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerNode>>();
        var nodeService = scope.ServiceProvider.GetRequiredService<NodeService>();

        foreach (var node in nodeRepo.Get().ToArray())
        {
            try
            {
                await nodeService.Boot(node);
            }
            catch (HttpRequestException e)
            {
                if(e.InnerException is SocketException socketException)
                    Logger.Warn($"Unable to auto boot node '{node.Name}'. Unable to reach the daemon: {socketException.Message}");
                else
                {
                    Logger.Warn($"An error occured while booting node '{node.Name}'");
                    Logger.Warn(e);
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"An error occured while booting node '{node.Name}'");
                Logger.Warn(e);
            }
        }
        
        Logger.Info("Booted all nodes");
    }
}