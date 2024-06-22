using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Helpers;
using Moonlight.Features.Servers.Attributes;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Events;
using Moonlight.Features.Servers.Extensions;
using Moonlight.Features.Servers.Http.Requests;
using Moonlight.Features.Servers.Models.Abstractions;

namespace Moonlight.Features.Servers.Http.Controllers;

[ApiController]
[Route("api/servers")]
[EnableNodeMiddleware]
public class ServersController : Controller
{
    private readonly Repository<Server> ServerRepository;
    private readonly Repository<ServerBackup> BackupRepository;
    private readonly ILogger<ServersController> Logger;
    private readonly ILogger<AdvancedWebsocketStream> WebSocketLogger;
    private readonly ServerEvents ServerEvents;

    public ServersController(Repository<Server> serverRepository, Repository<ServerBackup> backupRepository, ILogger<ServersController> logger, ILogger<AdvancedWebsocketStream> webSocketLogger, ServerEvents serverEvents)
    {
        ServerRepository = serverRepository;
        BackupRepository = backupRepository;
        Logger = logger;
        WebSocketLogger = webSocketLogger;
        ServerEvents = serverEvents;
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
        var websocketStream = new AdvancedWebsocketStream(WebSocketLogger, websocket);
        websocketStream.RegisterPacket<int>(1);
        websocketStream.RegisterPacket<ServerConfiguration>(2);
        
        // Read server data for the node
        var node = (HttpContext.Items["Node"] as ServerNode)!;

        // Load server data with including the relational data
        var servers = ServerRepository
            .Get()
            .Include(x => x.Allocations)
            .Include(x => x.Variables)
            .Include(x => x.MainAllocation)
            .Include(x => x.Network)
            .Include(x => x.Image)
            .Include(x => x.Image)
            .ThenInclude(x => x.DockerImages)
            .Where(x => x.Node.Id == node.Id)
            .ToArray();

        var serverConfigurations = new List<ServerConfiguration>();

        foreach (var server in servers)
        {
            try
            {
                serverConfigurations.Add(server.ToServerConfiguration());
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while sending server {serverId} (Image: {name}) to daemon. This may indicate a corrupt or broken image/server. Skipping this server. Error: {e}", server.Id, server.Image.Name, e);
            }
        }
        
        // Send the amount of configs the node will receive
        await websocketStream.SendPacket(servers.Length);

        // Send the server configurations
        foreach (var serverConfiguration in serverConfigurations)
            await websocketStream.SendPacket(serverConfiguration);

        await websocketStream.WaitForClose();

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
            .Include(x => x.Network)
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

    [HttpPost("{id:int}/backups/{backupId:int}")]
    public async Task<ActionResult> ReportBackupStatus(int id, int backupId, [FromBody] BackupStatus status)
    {
        var node = (HttpContext.Items["Node"] as ServerNode)!;
        
        var server = ServerRepository
            .Get()
            .Include(x => x.Backups)
            .Where(x => x.Node.Id == node.Id)
            .FirstOrDefault(x => x.Id == id);

        if (server == null)
            return NotFound();

        var backup = server.Backups.FirstOrDefault(x => x.Id == backupId);
        
        if (backup == null)
            return NotFound();
        
        if(!status.Successful)
            Logger.LogWarning("A node reported an error for a backup for the server {serverId}", server.Id);

        backup.Successful = status.Successful;
        backup.Completed = true;
        backup.Size = status.Size;
        
        BackupRepository.Update(backup);

        await ServerEvents.OnBackupCompleted.Invoke((server, backup));

        return Ok();
    }
}