using System.Net.WebSockets;
using MoonCore.Helpers;
using Moonlight.Features.Servers.Api.Packets;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Helpers;

public class ServerConsole : IDisposable
{
    public SmartEventHandler<ServerState> OnStateChange { get; set; }
    public SmartEventHandler<ServerStats> OnStatsChange { get; set; }
    public SmartEventHandler<string> OnNewMessage { get; set; }
    public SmartEventHandler OnDisconnected { get; set; }

    public ServerState State { get; private set; } = ServerState.Offline;
    public ServerStats Stats { get; private set; } = new();
    public DateTime LastStateChangeTimestamp { get; private set; } = DateTime.UtcNow;
    public string[] Messages => GetMessageCache();

    private readonly List<string> MessageCache = new();
    private readonly Server Server;
    private readonly ILogger<ServerConsole> Logger;
    private readonly ILogger<AdvancedWebsocketStream> AwsLogger;

    private ClientWebSocket WebSocket;
    private AdvancedWebsocketStream WebsocketStream;

    private CancellationTokenSource Cancellation = new();

    public ServerConsole(Server server, ILoggerFactory loggerFactory)
    {
        if (server.Node == null)
            throw new ArgumentNullException(nameof(server.Node));

        Server = server;
        
        Logger = loggerFactory.CreateLogger<ServerConsole>();
        AwsLogger = loggerFactory.CreateLogger<AdvancedWebsocketStream>();

        var eventHandlerLogger = loggerFactory.CreateLogger<SmartEventHandler>();
        OnStateChange = new(eventHandlerLogger);
        OnStatsChange = new(eventHandlerLogger);
        OnDisconnected = new(eventHandlerLogger);
        OnNewMessage = new(eventHandlerLogger);
    }

    public async Task Connect()
    {
        WebSocket = new();
        
        // Set auth header
        WebSocket.Options.SetRequestHeader("Authorization", Server.Node.Token);

        string wsUrl;

        if (Server.Node.Ssl)
            wsUrl = $"wss://{Server.Node.Fqdn}:{Server.Node.HttpPort}/servers/{Server.Id}/ws";
        else
            wsUrl = $"ws://{Server.Node.Fqdn}:{Server.Node.HttpPort}/servers/{Server.Id}/ws";

        await WebSocket.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
        WebsocketStream = new AdvancedWebsocketStream(AwsLogger, WebSocket);

        WebsocketStream.RegisterPacket<string>(1);
        WebsocketStream.RegisterPacket<ServerState>(2);
        WebsocketStream.RegisterPacket<ServerStats>(3);

        Task.Run(Worker);
    }

    private async Task Worker()
    {
        while (!Cancellation.IsCancellationRequested && WebSocket.State == WebSocketState.Open)
        {
            try
            {
                var packet = await WebsocketStream.ReceivePacket();

                if (packet == null)
                    continue;

                if (packet is string message)
                {
                    lock (MessageCache)
                    {
                        if (MessageCache.Count > 1000)
                            MessageCache.RemoveRange(0, 500);

                        MessageCache.Add(message);
                    }

                    await OnNewMessage.Invoke(message);
                }

                if (packet is ServerState state)
                {
                    State = state;
                    LastStateChangeTimestamp = DateTime.UtcNow;

                    await OnStateChange.Invoke(state);
                }

                if (packet is ServerStats stats)
                {
                    Stats = stats;

                    await OnStatsChange.Invoke(stats);
                }
            }
            catch (Exception e)
            {
                if(Cancellation.IsCancellationRequested)
                    break;
                
                if (e is WebSocketException)
                    Logger.LogWarning("Lost connection to daemon server websocket: {message}", e.Message);
                else
                {
                    Logger.LogWarning("Server console ws disconnected because of application error: {e}", e);
                }
                
                break;
            }
        }

        await OnDisconnected.Invoke();
    }

    public async Task Close()
    {
        if(!Cancellation.IsCancellationRequested)
            Cancellation.Cancel();

        if (WebSocket.State == WebSocketState.Open)
            await WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
    }

    private string[] GetMessageCache()
    {
        lock (MessageCache)
            return MessageCache.ToArray();
    }

    public async void Dispose()
    {
        MessageCache.Clear();
        
        await OnDisconnected.ClearSubscribers();
        await OnStateChange.ClearSubscribers();
        await OnStatsChange.ClearSubscribers();
        await OnNewMessage.ClearSubscribers();

        if (WebSocket.State == WebSocketState.Open)
            await WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        
        WebSocket.Dispose();
        
    }
}