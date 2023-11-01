using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Api;

namespace Moonlight.App.Http.Controllers.Api;

public class WebsocketController : Controller
{
    private readonly ApiManagementService ApiManagementService;
    
    public WebsocketController(ApiManagementService apiManagementService)
    {
        ApiManagementService = apiManagementService;
    }
    
    [Route("/api/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
            {
                await Echo(webSocket);
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    public async Task Echo(WebSocket webSocket)
    {
        var context = new ApiUserContext(webSocket);
        ApiManagementService.Contexts.Add(context);

        await webSocket.SendAsync(Encoding.UTF8.GetBytes("Hello World"), WebSocketMessageType.Text,
            true, CancellationToken.None);

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[1024 * 10];
                var data = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                buffer = buffer[..data.Count];

                await ApiManagementService.HandleRequest(context, buffer);
            }
        }
        catch (Exception)
        {
            
        }

        ApiManagementService.Contexts.Remove(context);
    }
}