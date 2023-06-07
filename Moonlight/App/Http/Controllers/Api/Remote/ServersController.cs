using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Events;
using Moonlight.App.Helpers;
using Moonlight.App.Helpers.Wings;
using Moonlight.App.Http.Resources.Wings;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Servers;
using Moonlight.App.Services;

namespace Moonlight.App.Http.Controllers.Api.Remote;

[Route("api/remote/servers")]
[ApiController]
public class ServersController : Controller
{
    private readonly WingsServerConverter Converter;
    private readonly ServerRepository ServerRepository;
    private readonly NodeRepository NodeRepository;
    private readonly EventSystem Event;

    public ServersController(
        WingsServerConverter converter,
        ServerRepository serverRepository,
        NodeRepository nodeRepository,
        EventSystem eventSystem)
    {
        Converter = converter;
        ServerRepository = serverRepository;
        NodeRepository = nodeRepository;
        Event = eventSystem;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResult<WingsServer>>> GetServers(
        [FromQuery(Name = "page")] int page,
        [FromQuery(Name = "per_page")] int perPage)
    {
        var tokenData = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var id = tokenData.Split(".")[0];
        var token = tokenData.Split(".")[1];

        var node = NodeRepository.Get().FirstOrDefault(x => x.TokenId == id);

        if (node == null)
            return NotFound();

        if (token != node.Token)
            return Unauthorized();

        var servers = ServerRepository
            .Get()
            .Include(x => x.Node)
            .Where(x => x.Node.Id == node.Id)
            .ToArray();

        List<WingsServer> wingsServers = new();
        int totalPages = 1;

        if (servers.Length > 0)
        {
            var slice = servers.Chunk(perPage).ToArray();
            var part = slice[page];

            foreach (var server in part)
            {
                wingsServers.Add(Converter.FromServer(server));
            }

            totalPages = slice.Length - 1;
        }

        await Event.Emit($"wings.{node.Id}.serverList", node);

        //Logger.Debug($"[BRIDGE] Node '{node.Name}' is requesting server list page {page} with {perPage} items per page");

        return PaginationResult<WingsServer>.CreatePagination(
            wingsServers.ToArray(),
            page,
            perPage,
            totalPages,
            servers.Length
        );
    }


    [HttpPost("reset")]
    public async Task<ActionResult> Reset()
    {
        var tokenData = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var id = tokenData.Split(".")[0];
        var token = tokenData.Split(".")[1];

        var node = NodeRepository.Get().FirstOrDefault(x => x.TokenId == id);

        if (node == null)
            return NotFound();

        if (token != node.Token)
            return Unauthorized();

        await Event.Emit($"wings.{node.Id}.stateReset", node);

        foreach (var server in ServerRepository
                     .Get()
                     .Include(x => x.Node)
                     .Where(x => x.Node.Id == node.Id)
                     .ToArray()
                )
        {
            if (server.Installing)
            {
                server.Installing = false;
                ServerRepository.Update(server);
            }
        }

        return Ok();
    }

    [HttpGet("{uuid}")]
    public async Task<ActionResult<WingsServer>> GetServer(Guid uuid)
    {
        var tokenData = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var id = tokenData.Split(".")[0];
        var token = tokenData.Split(".")[1];

        var node = NodeRepository.Get().FirstOrDefault(x => x.TokenId == id);

        if (node == null)
            return NotFound();

        if (token != node.Token)
            return Unauthorized();

        var server = ServerRepository.Get().FirstOrDefault(x => x.Uuid == uuid);

        if (server == null)
            return NotFound();

        await Event.Emit($"wings.{node.Id}.serverFetch", server);

        try //TODO: Remove
        {
            return Converter.FromServer(server);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("{uuid}/install")]
    public async Task<ActionResult<WingsServerInstall>> GetServerInstall(Guid uuid)
    {
        var tokenData = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var id = tokenData.Split(".")[0];
        var token = tokenData.Split(".")[1];

        var node = NodeRepository.Get().FirstOrDefault(x => x.TokenId == id);

        if (node == null)
            return NotFound();

        if (token != node.Token)
            return Unauthorized();

        var server = ServerRepository.Get().Include(x => x.Image).FirstOrDefault(x => x.Uuid == uuid);

        if (server == null)
            return NotFound();

        await Event.Emit($"wings.{node.Id}.serverInstallFetch", server);

        return new WingsServerInstall()
        {
            Entrypoint = server.Image.InstallEntrypoint,
            Script = server.Image.InstallScript!,
            Container_Image = server.Image.InstallDockerImage
        };
    }

    [HttpPost("{uuid}/install")]
    public async Task<ActionResult> SetInstallState(Guid uuid)
    {
        var tokenData = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var id = tokenData.Split(".")[0];
        var token = tokenData.Split(".")[1];

        var node = NodeRepository.Get().FirstOrDefault(x => x.TokenId == id);

        if (node == null)
            return NotFound();

        if (token != node.Token)
            return Unauthorized();

        var server = ServerRepository.Get().Include(x => x.Image).FirstOrDefault(x => x.Uuid == uuid);

        if (server == null)
            return NotFound();

        server.Installing = false;
        ServerRepository.Update(server);

        await Event.Emit($"wings.{node.Id}.serverInstallComplete", server);
        await Event.Emit($"server.{server.Uuid}.installComplete", server);

        return Ok();
    }
}