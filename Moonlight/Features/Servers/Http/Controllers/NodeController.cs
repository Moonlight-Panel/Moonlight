using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using Moonlight.Features.Servers.Api.Packets;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Extensions.Attributes;
using Moonlight.Features.Servers.Services;

namespace Moonlight.Features.Servers.Http.Controllers;

[ApiController]
[Route("api/servers/node")]
[EnableNodeMiddleware]
public class NodeController : Controller
{
    private readonly NodeService NodeService;
    private readonly ServerService ServerService;

    public NodeController(NodeService nodeService, ServerService serverService)
    {
        NodeService = nodeService;
        ServerService = serverService;
    }

    [HttpPost("notify/start")]
    public async Task<ActionResult> NotifyBootStart()
    {
        // Load node from request context
        var node = (HttpContext.Items["Node"] as ServerNode)!;

        await NodeService.Meta.Update(node.Id, meta => { meta.IsBooting = true; });

        return Ok();
    }

    [HttpPost("notify/finish")]
    public async Task<ActionResult> NotifyBootFinish()
    {
        // Load node from request context
        var node = (HttpContext.Items["Node"] as ServerNode)!;

        await NodeService.Meta.Update(node.Id, meta => { meta.IsBooting = false; });

        return Ok();
    }

    [HttpGet("ws")]
    public async Task<ActionResult> Ws()
    {
        // Validate if it is even a websocket connection
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("This endpoint is only available for websockets");

        // Accept websocket connection 
        var websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        // Build connection wrapper
        var wsPacketConnection = new WsPacketConnection(websocket);

        // Register packets
        await wsPacketConnection.RegisterPacket<ServerStateUpdate>("serverStateUpdate");
        await wsPacketConnection.RegisterPacket<ServerOutputMessage>("serverOutputMessage");

        while (websocket.State == WebSocketState.Open)
        {
            var packet = await wsPacketConnection.Receive();

            if (packet is ServerStateUpdate serverStateUpdate)
            {
                await ServerService.Meta.Update(serverStateUpdate.Id, meta =>
                {
                    meta.State = serverStateUpdate.State;
                    meta.LastChangeTimestamp = DateTime.UtcNow;
                });

                await (await ServerService.Meta.Get(serverStateUpdate.Id)).OnStateChanged.Invoke();
            }

            if (packet is ServerOutputMessage serverOutputMessage)
            {
                await ServerService.Meta.Update(serverOutputMessage.Id, meta =>
                {
                    lock (meta.ConsoleMessages)
                        meta.ConsoleMessages.Add(serverOutputMessage.Message);
                });

                await (await ServerService.Meta.Get(serverOutputMessage.Id)).OnConsoleMessage.Invoke(serverOutputMessage
                    .Message);
            }
        }

        await wsPacketConnection.Close();

        return Ok();
    }
}