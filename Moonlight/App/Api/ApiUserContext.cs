using System.Net.WebSockets;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Api;

public class ApiUserContext
{
    public ApiUserContext(WebSocket webSocket)
    {
        WebSocket = webSocket;
    }
    
    public User? User { get; set; }
    public WebSocket WebSocket { get; }
}