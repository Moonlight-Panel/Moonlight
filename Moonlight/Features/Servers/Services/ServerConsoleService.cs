using MoonCore.Attributes;
using Moonlight.Features.Servers.Api.Requests;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Extensions;
using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Services;

[Singleton]
public class ServerConsoleService
{
    private readonly IServiceProvider ServiceProvider;

    public ServerConsoleService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
    
    public async Task SendAction(Server server, PowerAction powerAction, bool runAsync = false)
    {
        using var httpClient = server.CreateHttpClient(ServiceProvider);
        await httpClient.Post($"servers/{server.Id}/power/{powerAction.ToString().ToLower()}?runAsync={runAsync}");
    }

    public async Task SendCommand(Server server, string command)
    {
        using var httpClient = server.CreateHttpClient(ServiceProvider);
        
        await httpClient.Post($"servers/{server.Id}/command", new SendCommand()
        {
            Command = command
        });
    }
}