using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Helpers;


using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Extensions;
using Moonlight.Features.Servers.Extensions.Attributes;
using Moonlight.Features.Servers.Models.Abstractions;

namespace Moonlight.Features.Servers.Http.Controllers;

[ApiController]
[Route("api/servers")]
[EnableNodeMiddleware]
public class ServersControllers : Controller
{
    private readonly Repository<Server> ServerRepository;

    public ServersControllers(Repository<Server> serverRepository)
    {
        ServerRepository = serverRepository;
    }

    [HttpGet("ws")]
    public async Task<ActionResult> GetAllServersWs()
    {
        // Validate if it is even a websocket connection
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("This endpoint is only available for websockets");

        // Accept websocket connection 
        var websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        
        // Build connection wrapper
        var wsPacketConnection = new WsPacketConnection(websocket);
        await wsPacketConnection.RegisterPacket<int>("amount");
        await wsPacketConnection.RegisterPacket<ServerConfiguration>("serverConfiguration");
        
        // Read server data for the node
        var node = (HttpContext.Items["Node"] as ServerNode)!;

        // Load server data with including the relational data
        var servers = ServerRepository
            .Get()
            .Include(x => x.Allocations)
            .Include(x => x.Variables)
            .Include(x => x.MainAllocation)
            .Include(x => x.Image)
            .ThenInclude(x => x.DockerImages)
            .Where(x => x.Node.Id == node.Id)
            .ToArray();

        // Convert the data to server configurations
        var serverConfigurations = servers
            .Select(x => x.ToServerConfiguration())
            .ToArray();
        
        // Send the amount of configs the node will receive
        await wsPacketConnection.Send(servers.Length);

        // Send the server configurations
        foreach (var serverConfiguration in serverConfigurations)
            await wsPacketConnection.Send(serverConfiguration);

        await wsPacketConnection.WaitForClose();

        return Ok();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServerConfiguration>> GetServerById(int id)
    {
        var node = (HttpContext.Items["Node"] as ServerNode)!;
        
        var server = ServerRepository
            .Get()
            .Include(x => x.Allocations)
            .Include(x => x.Variables)
            .Include(x => x.MainAllocation)
            .Include(x => x.Image)
            .ThenInclude(x => x.DockerImages)
            .Where(x => x.Node.Id == node.Id)
            .FirstOrDefault(x => x.Id == id);

        if (server == null)
            return NotFound();

        var configuration = server.ToServerConfiguration();

        return Ok(configuration);
    }
    
    [HttpGet("{id:int}/install")]
    public async Task<ActionResult<ServerInstallConfiguration>> GetServerInstallById(int id)
    {
        var node = (HttpContext.Items["Node"] as ServerNode)!;
        
        var server = ServerRepository
            .Get()
            .Include(x => x.Image)
            .Where(x => x.Node.Id == node.Id)
            .FirstOrDefault(x => x.Id == id);

        if (server == null)
            return NotFound();

        var configuration = server.ToServerInstallConfiguration();

        return Ok(configuration);
    }
}