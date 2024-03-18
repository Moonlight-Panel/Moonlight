using MoonCore.Helpers;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Exceptions;

namespace Moonlight.Features.Servers.Extensions;

public static class NodeExtensions
{
    public static HttpApiClient<NodeException> CreateHttpClient(this ServerNode node)
    {
        var protocol = node.UseSsl ? "https" : "http";
        var remoteUrl = $"{protocol}://{node.Fqdn}:{node.HttpPort}/";
        
        return new HttpApiClient<NodeException>(remoteUrl, node.Token);
    }
}