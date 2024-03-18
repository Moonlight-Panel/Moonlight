using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Helpers;
using Moonlight.Features.Servers.Api.Resources;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Extensions;

namespace Moonlight.Features.Servers.Services;

[Singleton]
public class NodeService
{
    private readonly IServiceProvider ServiceProvider;

    public NodeService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task Boot(ServerNode node)
    {
        using var httpClient = node.CreateHttpClient();
        await httpClient.Post("system/boot");
    }

    public Task BootAll()
    {
        using var scope = ServiceProvider.CreateScope();
        var nodeRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerNode>>();
        var nodes = nodeRepo.Get().ToArray();

        foreach (var node in nodes)
        {
            // Run in a new task to stop preventing a offline node's http timeout from other nodes from being booted
            Task.Run(async () =>
            {
                try
                {
                    await Boot(node);
                }
                catch (Exception e)
                {
                    //TODO: Add http exception check to reduce error logs
                    
                    Logger.Warn($"An error occured while booting node '{node.Name}'");
                    Logger.Warn(e);
                }
            });
        }
        
        return Task.CompletedTask;
    }

    public async Task<SystemStatus> GetStatus(ServerNode node)
    {
        using var httpClient = node.CreateHttpClient();
        return await httpClient.Get<SystemStatus>("system/info");
    }

    public async Task<string> GetLogs(ServerNode node)
    {
        using var httpClient = node.CreateHttpClient();
        return await httpClient.GetAsString("system/info/logs");
    }
}