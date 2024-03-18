using Microsoft.AspNetCore.Mvc;
using Moonlight.Features.Servers.Attributes;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Services;

namespace Moonlight.Features.Servers.Http.Controllers;

[ApiController]
[Route("api/servers/node")]
[EnableNodeMiddleware]
public class NodeController : Controller
{
    private readonly NodeService NodeService;

    public NodeController(NodeService nodeService)
    {
        NodeService = nodeService;
    }

    [HttpPost("notify/start")]
    public async Task<ActionResult> NotifyBootStart()
    {
        // Load node from request context
        var node = (HttpContext.Items["Node"] as ServerNode)!;

        //TODO: Save state

        return Ok();
    }

    [HttpPost("notify/finish")]
    public async Task<ActionResult> NotifyBootFinish()
    {
        // Load node from request context
        var node = (HttpContext.Items["Node"] as ServerNode)!;

        //TODO: Save state

        return Ok();
    }
}