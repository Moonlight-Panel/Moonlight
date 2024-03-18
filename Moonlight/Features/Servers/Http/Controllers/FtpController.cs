using Microsoft.AspNetCore.Mvc;
using MoonCore.Services;
using Moonlight.Core.Services;
using Moonlight.Features.Servers.Attributes;
using Moonlight.Features.Servers.Http.Requests;
using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Http.Controllers;

[ApiController]
[Route("api/servers/ftp")]
[EnableNodeMiddleware]
public class FtpController : Controller
{
    private readonly IServiceProvider ServiceProvider;
    private readonly JwtService<ServersJwtType> JwtService;

    public FtpController(
        IServiceProvider serviceProvider,
        JwtService<ServersJwtType> jwtService)
    {
        ServiceProvider = serviceProvider;
        JwtService = jwtService;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] FtpLogin login)
    {
        return Ok();

        /*

        // If it looks like a jwt, try authenticate it
        if (await TryJwtLogin(login))
            return Ok();

        // Search for user
        var userRepo = ServiceProvider.GetRequiredService<Repository<User>>();
        var user = userRepo
            .Get()
            .FirstOrDefault(x => x.Username == login.Username);

        // Unknown user
        if (user == null)
            return StatusCode(403);

        // Check password
        if (!HashHelper.Verify(login.Password, user.Password))
        {
            Logger.Warn($"A failed login attempt via ftp has occured. Username: '{login.Username}', Server Id: '{login.ServerId}'");
            return StatusCode(403);
        }

        // Load node from context
        var node = HttpContext.Items["Node"] as ServerNode;

        // Load server from db
        var serverRepo = ServiceProvider.GetRequiredService<Repository<Server>>();

        */
    }
    
    private async Task<bool> TryJwtLogin(FtpLogin login)
    {
        if (!await JwtService.Validate(login.Password, ServersJwtType.FtpServerLogin))
            return false;
        
        var data = await JwtService.Decode(login.Password);

        if (!data.ContainsKey("ServerId"))
            return false;

        if (!int.TryParse(data["ServerId"], out int serverId))
            return false;

        return login.ServerId == serverId;
    }
}