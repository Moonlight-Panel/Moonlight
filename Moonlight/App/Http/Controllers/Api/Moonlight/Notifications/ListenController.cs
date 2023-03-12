using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities.Notification;
using Moonlight.App.Models.Notifications;
using Moonlight.App.Repositories;
using Moonlight.App.Services;
using Moonlight.App.Services.Notifications;
using Moonlight.App.Services.Sessions;
using Newtonsoft.Json;

namespace Moonlight.App.Http.Controllers.Api.Moonlight.Notifications;

public class ListenController : ControllerBase
{
    internal WebSocket ws;
    private bool active = true;
    private bool isAuth = false;
    private NotificationClient Client;
    
    private readonly IdentityService IdentityService;
    private readonly NotificationRepository NotificationRepository;
    private readonly OneTimeJwtService OneTimeJwtService;
    private readonly NotificationClientService NotificationClientService;
    private readonly NotificationServerService NotificationServerService;

    public ListenController(IdentityService identityService, 
        NotificationRepository notificationRepository, 
        OneTimeJwtService oneTimeJwtService, 
        NotificationClientService notificationClientService,
        NotificationServerService notificationServerService)
    {
        IdentityService = identityService;
        NotificationRepository = notificationRepository;
        OneTimeJwtService = oneTimeJwtService;
        NotificationClientService = notificationClientService;
        NotificationServerService = notificationServerService;
    }
    
    [Route("/api/moonlight/notifications/listen")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            ws = webSocket;
            await Echo();
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }

    private async Task Echo()
    {
        while (active)
        {
            byte[] bytes = new byte[1024 * 16]; 
            var asg = new ArraySegment<byte>(bytes);
            var res = await ws.ReceiveAsync(asg, CancellationToken.None);

            var text = Encoding.UTF8.GetString(bytes).Trim('\0');

            var obj = JsonConvert.DeserializeObject<BasicWSModel>(text);

            if (!string.IsNullOrWhiteSpace(obj.Action))
            {
                await HandleRequest(text, obj.Action);
            }
            
            active = ws.State == WebSocketState.Open;
        }
    }

    private async Task HandleRequest(string text, string action)
    {
        if (!isAuth && action == "login")
            await Login(text);
        else if (!isAuth)
            await ws.SendAsync(Encoding.UTF8.GetBytes("{\"error\": \"Unauthorised\"}"), WebSocketMessageType.Text,
                WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
        else switch (action)
        {
            case "received":
                await Received(text);
                break;
            case "read":
                await Read(text);
                break;
            default:
                break;
        }
    }

    private async Task Login(string json)
    {
        var jwt = JsonConvert.DeserializeObject<Login>(json).token;
        
        var dict = await OneTimeJwtService.Validate(jwt);

        if (dict == null)
        {
            string error = "{\"status\":false}";
            var bytes = Encoding.UTF8.GetBytes(error);
            await ws.SendAsync(bytes, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
            return;
        }

        var _clientId = dict["clientId"];
        var clientId = int.Parse(_clientId);

        var client = NotificationRepository.GetClients().Include(x => x.User).First(x => x.Id == clientId);

        Client = client;
        await InitWebsocket();
        
        string success = "{\"status\":true}";
        var byt = Encoding.UTF8.GetBytes(success);
        await ws.SendAsync(byt, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
    }

    private async Task InitWebsocket()
    {
        NotificationClientService.listenController = this;
        NotificationClientService.WebsocketReady(Client);

        isAuth = true;
    }

    private async Task Received(string json)
    {
        var id = JsonConvert.DeserializeObject<NotificationById>(json).notification;
        
        //TODO: Implement ws notification received
    }

    private async Task Read(string json)
    {
        var id = JsonConvert.DeserializeObject<NotificationById>(json).notification;

        await NotificationServerService.SendAction(NotificationClientService.User,
            JsonConvert.SerializeObject(new NotificationById() {Action = "hide", notification = id}));
    }
}