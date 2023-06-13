using System.Net.WebSockets;
using System.Text;
using Logging.Net;
using Moonlight.App.Helpers.Wings.Data;
using Moonlight.App.Helpers.Wings.Enums;
using Moonlight.App.Helpers.Wings.Events;
using Newtonsoft.Json;
using ConsoleMessage = Moonlight.App.Helpers.Wings.Data.ConsoleMessage;

namespace Moonlight.App.Helpers.Wings;

public class WingsConsole : IDisposable
{
    private ClientWebSocket WebSocket;
    public List<ConsoleMessage> Messages;
    private Task? ConsoleTask;

    private string Socket = "";
    private string Origin = "";
    private string Token = "";
    
    private bool Disconnecting;

    public ConsoleState ConsoleState { get; private set; }
    public ServerState ServerState { get; private set; }
    public ServerResource Resource { get; private set; }

    public EventHandler<ConsoleState> OnConsoleStateUpdated { get; set; }
    public EventHandler<ServerState> OnServerStateUpdated { get; set; }
    public EventHandler<ServerResource> OnResourceUpdated { get; set; }
    public EventHandler<ConsoleMessage> OnMessage { get; set; }
    public Func<WingsConsole, Task<string>> OnRequestNewToken { get; set; }

    public WingsConsole()
    {
        ConsoleState = ConsoleState.Disconnected;
        ServerState = ServerState.Offline;
        Messages = new();
        
        Resource = new()
        {
            Network = new()
            {
                RxBytes = 0,
                TxBytes = 0
            },
            State = "offline",
            Uptime = 0,
            CpuAbsolute = 0,
            DiskBytes = 0,
            MemoryBytes = 0,
            MemoryLimitBytes = 0
        };
    }

    public Task Connect(string origin, string socket, string token)
    {
        Disconnecting = false;
        WebSocket = new();
        ConsoleState = ConsoleState.Disconnected;
        ServerState = ServerState.Offline;
        Messages = new();
        
        Resource = new()
        {
            Network = new()
            {
                RxBytes = 0,
                TxBytes = 0
            },
            State = "offline",
            Uptime = 0,
            CpuAbsolute = 0,
            DiskBytes = 0,
            MemoryBytes = 0,
            MemoryLimitBytes = 0
        };
        
        Socket = socket;
        Origin = origin;
        Token = token;

        WebSocket.Options.SetRequestHeader("Origin", Origin);
        WebSocket.Options.SetRequestHeader("Authorization", "Bearer " + Token);

        ConsoleTask = Task.Run(async () =>
        {
            try
            {
                await Work();
            }
            catch (JsonReaderException)
            {
                // ignore
            }
            catch (Exception e)
            {
                Logger.Warn("Error connecting to wings console");
                Logger.Warn(e);
            }
        });

        return Task.CompletedTask;
    }

    private async Task Work()
    {
        await UpdateConsoleState(ConsoleState.Connecting);

        await WebSocket.ConnectAsync(
            new Uri(Socket),
            CancellationToken.None
        );

        if (WebSocket.State != WebSocketState.Connecting && WebSocket.State != WebSocketState.Open)
        {
            await SaveMessage("Unable to connect to websocket", true);
            await UpdateConsoleState(ConsoleState.Disconnected);
            return;
        }

        await UpdateConsoleState(ConsoleState.Connected);

        await Send(new SendTokenEvent()
        {
            Args = { Token }
        });

        while (WebSocket.State == WebSocketState.Open)
        {
            try
            {
                var raw = await ReceiveRaw();
                
                if(string.IsNullOrEmpty(raw))
                    continue;
                
                var eventData = JsonConvert.DeserializeObject<BaseEvent>(raw);

                if (eventData == null)
                {
                    await SaveMessage("Unable to parse event", true);
                    continue;
                }

                switch (eventData.Event)
                {
                    case "jwt error":
                        if (WebSocket != null)
                        {
                            if (WebSocket.State == WebSocketState.Connecting || WebSocket.State == WebSocketState.Open)
                                await WebSocket.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
            
                            WebSocket.Dispose();
                        }
                        
                        await UpdateServerState(ServerState.Offline);
                        await UpdateConsoleState(ConsoleState.Disconnected);

                        await SaveMessage("Received a jwt error. Disconnected", true);
                        break;

                    case "token expired":
                        if (WebSocket != null)
                        {
                            if (WebSocket.State == WebSocketState.Connecting || WebSocket.State == WebSocketState.Open)
                                await WebSocket.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
            
                            WebSocket.Dispose();
                        }
                        
                        await UpdateServerState(ServerState.Offline);
                        await UpdateConsoleState(ConsoleState.Disconnected);

                        await SaveMessage("Token expired", true);
                        
                        break;

                    case "token expiring":
                        await SaveMessage("Token will expire soon. Generating a new one", true);

                        Token = await OnRequestNewToken.Invoke(this);

                        await Send(new SendTokenEvent()
                        {
                            Args = { Token }
                        });
                        break;

                    case "auth success":
                        // Send intents
                        await SendRaw("{\"event\":\"send logs\",\"args\":[null]}");
                        await SendRaw("{\"event\":\"send stats\",\"args\":[null]}");
                        break;

                    case "stats":
                        var stats = JsonConvert.DeserializeObject<ServerResource>(eventData.Args[0]);

                        if (stats == null)
                            break;

                        var serverState = ParseServerState(stats.State);

                        if (ServerState != serverState)
                            await UpdateServerState(serverState);

                        await UpdateResource(stats);
                        break;

                    case "status":
                        var serverStateParsed = ParseServerState(eventData.Args[0]);

                        if (ServerState != serverStateParsed)
                            await UpdateServerState(serverStateParsed);
                        break;

                    case "console output":
                        foreach (var line in eventData.Args)
                        {
                            await SaveMessage(line);
                        }

                        break;

                    case "install output":
                        foreach (var line in eventData.Args)
                        {
                            await SaveMessage(line);
                        }

                        break;

                    case "daemon message":
                        foreach (var line in eventData.Args)
                        {
                            await SaveMessage(line);
                        }

                        break;

                    case "install started":
                        await UpdateServerState(ServerState.Installing);
                        break;

                    case "install completed":
                        await UpdateServerState(ServerState.Offline);
                        break;
                }
            }
            catch (Exception e)
            {
                if (!Disconnecting)
                {
                    Logger.Warn("Error while performing websocket actions");
                    Logger.Warn(e);

                    await SaveMessage("A unknown error occured while processing websocket", true);
                }
            }
        }
    }

    private Task UpdateConsoleState(ConsoleState consoleState)
    {
        ConsoleState = consoleState;
        OnConsoleStateUpdated?.Invoke(this, consoleState);

        return Task.CompletedTask;
    }
    private Task UpdateServerState(ServerState serverState)
    {
        ServerState = serverState;
        OnServerStateUpdated?.Invoke(this, serverState);

        return Task.CompletedTask;
    }
    private Task UpdateResource(ServerResource resource)
    {
        Resource = resource;
        OnResourceUpdated?.Invoke(this, Resource);

        return Task.CompletedTask;
    }

    private Task SaveMessage(string content, bool internalMessage = false)
    {
        var msg = new ConsoleMessage()
        {
            Content = content,
            IsInternal = internalMessage
        };

        lock (Messages)
        {
            Messages.Add(msg);
        }

        OnMessage?.Invoke(this, msg);

        return Task.CompletedTask;
    }
    
    private ServerState ParseServerState(string raw)
    {
        switch (raw)
        {
            case "offline":
                return ServerState.Offline;
            case "starting":
                return ServerState.Starting;
            case "running":
                return ServerState.Running;
            case "stopping":
                return ServerState.Stopping;
            case "installing":
                return ServerState.Installing;
            default:
                return ServerState.Offline;
        }
    }
    
    public async Task EnterCommand(string content)
    {
        if (ConsoleState == ConsoleState.Connected)
        {
            await SendRaw("{\"event\":\"send command\",\"args\":[\"" + content + "\"]}");
        }
    }

    public async Task SetPowerState(string state)
    {
        if (ConsoleState == ConsoleState.Connected)
        {
            await SendRaw("{\"event\":\"set state\",\"args\":[\"" + state + "\"]}");
        }
    }

    private async Task Send(object data)
    {
        await SendRaw(JsonConvert.SerializeObject(data));
    }
    
    private async Task SendRaw(string data)
    {
        if (WebSocket.State == WebSocketState.Open)
        {
            byte[] byteContentBuffer = Encoding.UTF8.GetBytes(data);
            await WebSocket.SendAsync(new ArraySegment<byte>(byteContentBuffer), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }
    }

    private async Task<string> ReceiveRaw()
    {
        ArraySegment<byte> receivedBytes = new ArraySegment<byte>(new byte[1024]);
        WebSocketReceiveResult result = await WebSocket.ReceiveAsync(receivedBytes, CancellationToken.None);
        return Encoding.UTF8.GetString(receivedBytes.Array!, 0, result.Count);
    }

    public async Task Disconnect()
    {
        Disconnecting = true;
        Messages.Clear();

        if (WebSocket != null)
        {
            if (WebSocket.State == WebSocketState.Connecting || WebSocket.State == WebSocketState.Open)
                await WebSocket.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
            
            WebSocket.Dispose();
        }

        if(ConsoleTask != null && ConsoleTask.IsCompleted)
            ConsoleTask.Dispose();
    }

    public void Dispose()
    {
        Disconnecting = true;
        Messages.Clear();

        if (WebSocket != null)
        {
            if (WebSocket.State == WebSocketState.Connecting || WebSocket.State == WebSocketState.Open)
                WebSocket.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None).Wait();
            
            WebSocket.Dispose();
        }
        
        if(ConsoleTask != null && ConsoleTask.IsCompleted)
            ConsoleTask.Dispose();
    }
}