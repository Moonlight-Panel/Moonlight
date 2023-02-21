using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Http.Requests.Wings;
using Moonlight.App.Http.Resources.Wings;
using Moonlight.App.Repositories;
using Moonlight.App.Services;

namespace Moonlight.App.Http.Controllers.Api.Remote;

[ApiController]
[Route("api/remote/sftp/auth")]
public class SftpAuthController : Controller
{
    private readonly ServerService ServerService;
    private readonly NodeRepository NodeRepository;

    public SftpAuthController(
        ServerService serverService, 
        NodeRepository nodeRepository)
    {
        ServerService = serverService;
        NodeRepository = nodeRepository;
    }

    [HttpPost]
    public async Task<ActionResult<SftpLoginResult>> Login(SftpLoginRequest request)
    {
        var tokenData = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var tokenId = tokenData.Split(".")[0];
        var token = tokenData.Split(".")[1];

        var node = NodeRepository.Get().FirstOrDefault(x => x.TokenId == tokenId);

        if (node == null)
            return NotFound();

        if (token != node.Token)
            return Unauthorized();
        
        if (request.Type == "public_key") // Deny public key authentication, because moonlight does not implement that
        {
            return StatusCode(403);
        }

        // Parse the username
        var parts = request.Username.Split(".");

        if (parts.Length < 2)
            return BadRequest();

        if (!int.TryParse(parts[0], out int id))
            return BadRequest();

        if (!int.TryParse(parts[1], out int serverId))
            return BadRequest();

        try
        {
            var server = await ServerService.SftpServerLogin(serverId, id, request.Password);

            return Ok(new SftpLoginResult()
            {
                Server = server.Uuid.ToString(),
                User = "",
                Permissions = new()
                {
                    "control.console",
                    "control.start",
                    "control.stop",
                    "control.restart",
                    "websocket.connect",
                    "file.create",
                    "file.read",
                    "file.read-content",
                    "file.update",
                    "file.delete",
                    "file.archive",
                    "file.sftp",
                    "user.create",
                    "user.read",
                    "user.update",
                    "user.delete",
                    "backup.create",
                    "backup.read",
                    "backup.delete",
                    "backup.download",
                    "backup.restore",
                    "allocation.read",
                    "allocation.create",
                    "allocation.update",
                    "allocation.delete",
                    "startup.read",
                    "startup.update",
                    "startup.docker-image",
                    "database.create",
                    "database.read",
                    "database.update",
                    "database.delete",
                    "database.view_password",
                    "schedule.create",
                    "schedule.read",
                    "schedule.update",
                    "schedule.delete",
                    "settings.rename",
                    "settings.reinstall"
                }
            });
        }
        catch (Exception e)
        {
            // Most of the exception here will be because of stuff like a invalid server id and simular things
            // so we ignore them and return 403
            return StatusCode(403);
        }
    }
}