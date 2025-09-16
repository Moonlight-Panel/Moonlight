using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Moonlight.ApiServer.Http.Hubs;

[Authorize(Policy = "permissions:admin.system.diagnose")]
public class DiagnoseHub : Hub
{
    [HubMethodName("Ping")]
    public async Task Ping()
    {
        await Clients.All.SendAsync("Pong");
    }
}