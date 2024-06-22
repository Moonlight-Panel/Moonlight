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
    private readonly ILogger<NodeService> Logger;

    public NodeService(IServiceProvider serviceProvider, ILogger<NodeService> logger)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
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
                    
                    Logger.LogWarning("An error occured while booting node '{name}': {e}", node.Name, e);
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