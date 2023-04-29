using Logging.Net;
using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Database.Entities;
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
    private readonly DdosAttackRepository DdosAttackRepository;

    public DdosController(NodeRepository nodeRepository, MessageService messageService, DdosAttackRepository ddosAttackRepository)
    {
        NodeRepository = nodeRepository;
        MessageService = messageService;
        DdosAttackRepository = ddosAttackRepository;
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

        var ddosAttack = new DdosAttack()
        {
            Ongoing = ddosStatus.Ongoing,
            Data = ddosStatus.Data,
            Ip = ddosStatus.Ip,
            Node = node
        };

        ddosAttack = DdosAttackRepository.Add(ddosAttack);

        await MessageService.Emit("node.ddos", ddosAttack);
        
        return Ok();
    }
}