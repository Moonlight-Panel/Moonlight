using Logging.Net;
using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Http.Requests.Daemon;
using Moonlight.App.Repositories;
using Moonlight.App.Services;

namespace Moonlight.App.Http.Controllers.Api.Remote;

[ApiController]
[Route("api/remote/ddos")]
public class DdosController : Controller
{
    private readonly NodeRepository NodeRepository;
    private readonly MessageService MessageService;

    public DdosController(NodeRepository nodeRepository, MessageService messageService)
    {
        NodeRepository = nodeRepository;
        MessageService = messageService;
    }

    [HttpPost("update")]
    public async Task<ActionResult> Update([FromBody] DdosStatus ddosStatus)
    {
        var tokenData = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var id = tokenData.Split(".")[0];
        var token = tokenData.Split(".")[1];

        var node = NodeRepository.Get().FirstOrDefault(x => x.TokenId == id);

        if (node == null)
            return NotFound();
        
        if (token != node.Token)
            return Unauthorized();

        await MessageService.Emit("node.ddos", ddosStatus);
        
        return Ok();
    }
}