using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Helpers;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Services.Utils;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Extensions.Attributes;
using Moonlight.Features.Servers.Http.Requests;
using Moonlight.Features.ServiceManagement.Services;

namespace Moonlight.Features.Servers.Http.Controllers;

[ApiController]
[Route("api/servers/ftp")]
[EnableNodeMiddleware]
public class FtpController : Controller
{
    private readonly IServiceProvider ServiceProvider;
    private readonly JwtService JwtService;

    public FtpController(
        IServiceProvider serviceProvider,
        JwtService jwtService)
    {
        ServiceProvider = serviceProvider;
        JwtService = jwtService;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] FtpLogin login)
    {
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
        var server = serverRepo
            .Get()
            .Include(x => x.Service)
            .FirstOrDefault(x => x.Id == login.ServerId && x.Node.Id == node!.Id);

        // Unknown server or wrong node?
        if (server == null)
            return StatusCode(403);
        
        var serviceManageService = ServiceProvider.GetRequiredService<ServiceManageService>();

        // Has user access to this server?
        if (await serviceManageService.CheckAccess(server.Service, user))
            return Ok();

        return StatusCode(403);
    }
    
    private async Task<bool> TryJwtLogin(FtpLogin login)
    {
        if (!await JwtService.Validate(login.Password, "FtpServerLogin"))
            return false;
        
        var data = await JwtService.Decode(login.Password);

        if (!data.ContainsKey("ServerId"))
            return false;

        if (!int.TryParse(data["ServerId"], out int serverId))
            return false;

        return login.ServerId == serverId;
    }
}