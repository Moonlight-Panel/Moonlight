using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Helpers;


using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Exceptions;
using Moonlight.Features.Servers.Helpers;
using Moonlight.Features.Servers.Models.Abstractions;
using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Services;

[Singleton]
public class ServerService
{
    public readonly MetaCache<ServerMeta> Meta = new();

    private readonly IServiceProvider ServiceProvider;

    public ServerService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task Sync(Server server)
    {
        using var httpClient = CreateHttpClient(server);
        await httpClient.Post($"servers/{server.Id}/sync");
    }

    public async Task SyncDelete(Server server)
    {
        
    }

    public async Task SendPowerAction(Server server, PowerAction powerAction)
    {
        using var httpClient = CreateHttpClient(server);
        await httpClient.Post($"servers/{server.Id}/power/{powerAction.ToString().ToLower()}");
    }

    private HttpApiClient<NodeException> CreateHttpClient(Server server)
    {
        using var scope = ServiceProvider.CreateScope();
        var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();

        var serverWithNode = serverRepo
            .Get()
            .Include(x => x.Node)
            .First(x => x.Id == server.Id);

        var protocol = serverWithNode.Node.UseSsl ? "https" : "http";
        var remoteUrl = $"{protocol}://{serverWithNode.Node.Fqdn}:{serverWithNode.Node.HttpPort}/";
        
        return new HttpApiClient<NodeException>(remoteUrl, serverWithNode.Node.Token);
    }
}