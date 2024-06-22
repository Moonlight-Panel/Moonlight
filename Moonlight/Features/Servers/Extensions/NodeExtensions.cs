using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Exceptions;
using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Extensions;

public static class NodeExtensions
{
    public static HttpApiClient<NodeException> CreateHttpClient(this ServerNode node)
    {
        var protocol = node.Ssl ? "https" : "http";
        var remoteUrl = $"{protocol}://{node.Fqdn}:{node.HttpPort}/";
        
        return new HttpApiClient<NodeException>(remoteUrl, node.Token);
    }

    public static JwtService<ServersJwtType> CreateJwtService(this ServerNode node, ILoggerFactory factory)
    {
        return node.CreateJwtService(factory.CreateLogger<JwtService<ServersJwtType>>());
    }
    
    public static JwtService<ServersJwtType> CreateJwtService(this ServerNode node, ILogger<JwtService<ServersJwtType>> logger)
    {
        return new JwtService<ServersJwtType>(node.Token, logger);
    }
}