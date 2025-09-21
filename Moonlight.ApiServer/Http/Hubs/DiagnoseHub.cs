using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Moonlight.ApiServer.Http.Hubs;

[Authorize(Policy = "permissions:admin.system.diagnose")]
public class DiagnoseHub : Hub
{
    [HubMethodName("Ping")]
    public async Task PingAsync()
    {
        await Clients.All.SendAsync("Pong");
    }
}