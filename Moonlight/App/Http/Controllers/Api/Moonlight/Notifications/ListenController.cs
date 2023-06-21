using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Notification;
using Moonlight.App.Models.Notifications;
using Moonlight.App.Repositories;
using Moonlight.App.Services;
using Moonlight.App.Services.Notifications;
using Newtonsoft.Json;

namespace Moonlight.App.Http.Controllers.Api.Moonlight.Notifications;

[ApiController]
[Route("api/moonlight/notification/listen")]
public class ListenController : Controller
{
    private WebSocket WebSocket;
    private NotificationClient Client;
    private CancellationTokenSource CancellationTokenSource = new();

    private User? CurrentUser;

    private readonly OneTimeJwtService OneTimeJwtService;
    private readonly NotificationServerService NotificationServerService;
    private readonly Repository<NotificationClient> NotificationClientRepository;

    public ListenController(
        OneTimeJwtService oneTimeJwtService,
        NotificationServerService notificationServerService, Repository<NotificationClient> notificationClientRepository)
    {
        OneTimeJwtService = oneTimeJwtService;
        NotificationServerService = notificationServerService;
        NotificationClientRepository = notificationClientRepository;
    }

    [Route("/api/moonlight/notifications/listen")]
    public async Task<ActionResult> Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            WebSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            await ProcessWebsocket();

            return new EmptyResult();
        }
        else
        {
            return StatusCode(400);
        }
    }

    private async Task ProcessWebsocket()
    {
        while (!CancellationTokenSource.Token.IsCancellationRequested && WebSocket.State == WebSocketState.Open)
        {
            try
            {
                byte[] buffer = new byte[1024 * 16];
                _ = await WebSocket.ReceiveAsync(buffer, CancellationTokenSource.Token);
                var text = Encoding.UTF8.GetString(buffer).Trim('\0');

                var basicWsModel = JsonConvert.DeserializeObject<BasicWSModel>(text) ?? new();

                if (!string.IsNullOrWhiteSpace(basicWsModel.Action))
                {
                    await HandleRequest(text, basicWsModel.Action);
                }

                if (WebSocket.State != WebSocketState.Open)
                {
                    CancellationTokenSource.Cancel();
                }
            }
            catch (WebSocketException e)
            {
                CancellationTokenSource.Cancel();
            }
        }

        await NotificationServerService.UnRegisterClient(Client);
    }

    private async Task HandleRequest(string text, string action)
    {
        if (CurrentUser == null && action != "login")
        {
            await Send("{\"error\": \"Unauthorised\"}");
        }

        switch (action)
        {
            case "login":
                await Login(text);
                break;
            case "received":
                await Received(text);
                break;
            case "read":
                await Read(text);
                break;
        }
    }

    private async Task Send(string text)
    {
        await WebSocket.SendAsync(
            Encoding.UTF8.GetBytes(text),
            WebSocketMessageType.Text,
            WebSocketMessageFlags.EndOfMessage, CancellationTokenSource.Token
        );
    }

    private async Task Login(string json)
    {
        var loginModel = JsonConvert.DeserializeObject<Login>(json) ?? new();

        var dict = await OneTimeJwtService.Validate(loginModel.Token);

        if (dict == null)
        {
            await Send("{\"status\":false}");
            return;
        }

        if (!int.TryParse(dict["clientId"], out int clientId))
        {
            await Send("{\"status\":false}");
            return;
        }

        Client = NotificationClientRepository
            .Get()
            .Include(x => x.User)
            .First(x => x.Id == clientId);

        CurrentUser = Client.User;

        await NotificationServerService.RegisterClient(WebSocket, Client);

        await Send("{\"status\":true}");
    }

    private async Task Received(string json)
    {
        var id = JsonConvert.DeserializeObject<NotificationById>(json).Notification;

        //TODO: Implement ws notification received
    }

    private async Task Read(string json)
    {
        var model = JsonConvert.DeserializeObject<NotificationById>(json) ?? new();

        await NotificationServerService.SendAction(
            CurrentUser!,
            JsonConvert.SerializeObject(
                new NotificationById()
                {
                    Action = "hide", Notification = model.Notification
                }
            )
        );
    }
}